using Android.App;
using Android.OS;
using Android.Service.Notification;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios.Notificaciones
{
    internal class CatcherHelper : Java.Lang.Object
    {
        public static NotificationAdapter notificationAdapter;
        public static List<OpenNotification> StatusBarNotifications { get; internal set; } = new List<OpenNotification>();

        public static event EventHandler<NotificationRemovedEventArgs> NotificationRemoved;

        public static event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;

        //So it can grab it from here.

        /// <summary>
        /// Constructor of the Class
        /// </summary>
        /// <param name="statusBarNotifications">This list is sent by Catcher, and is used to fill the Adapter
        /// that the RecyclerView will use, it is tighly coupled with that adapter.
        /// </param>
        public CatcherHelper(List<StatusBarNotification> statusBarNotifications)
        {
            foreach (var sbn in statusBarNotifications)
            {
                StatusBarNotifications.Add(new OpenNotification(sbn));
            }
            notificationAdapter = new NotificationAdapter(StatusBarNotifications);
            if (statusBarNotifications.Count > 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = true
                });
            }
        }

        public void OnNotificationPosted(OpenNotification sbn)
        {
            if (sbn == null) { return; }
            //This is the notification of 'LiveDisplay is showing above other apps'
            //Simply let's ignore it, because it's annoying. (Anyway, the user couldn't care less about this notification tbh)
            if (sbn.GetPackage() == "android" && sbn.GetTag() == "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay")
                return;

            if (sbn.IsSummary())
                return; //Ignore the summary notification. (it causes redundancy) anyway, In an ideal scenario I should hide this notification instead
            //of ignoring it.

            var blockingstatus = Blacklist.ReturnBlockLevel(sbn.GetPackage());

            if (!blockingstatus.HasFlag(LevelsOfAppBlocking.Blacklisted))
            {
                if (!blockingstatus.HasFlag(LevelsOfAppBlocking.BlockInAppOnly))
                {
                    bool causesWakeUp = false;
                    if (sbn.GetNotificationPriority() >= (int)NotificationPriority.Default) //Solves a issue where non important notifications also turn on screen.
                        //anyway this is a hotfix, a better method shoudl be used to improve the blacklist/the importance of notifications.
                        causesWakeUp = true;
                    else
                        causesWakeUp = false;


                    int index = GetNotificationPosition(sbn); //Tries to get the index of a possible already existing notification in the list of notif.
                    if (index >= 0)
                    {
                        //It exists within the list.
                        //SO it should be updated.

                        StatusBarNotifications.RemoveAt(index);
                        StatusBarNotifications.Add(sbn);
                        using (var h = new Handler(Looper.MainLooper))
                            h.Post(() => { notificationAdapter.NotifyItemChanged(index); });

                            OnNotificationPosted(false, sbn, true);
                        
                    }
                    else
                    {
                        StatusBarNotifications.Add(sbn);

                        using (var h = new Handler(Looper.MainLooper))
                            h.Post(() => { notificationAdapter.NotifyItemInserted(StatusBarNotifications.Count); });
                        OnNotificationPosted(causesWakeUp, sbn, false);
                    }
                }
            }
            else
            {
                var notificationSlave = NotificationSlave.NotificationSlaveInstance();
                if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
                {
                    notificationSlave.CancelNotification(sbn.GetKey());
                }
                else
                {
                    notificationSlave.CancelNotification(sbn.GetPackage(), sbn.GetTag(), sbn.GetId());
                }
            }


            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = StatusBarNotifications.Count > 0
            }) ;
            
        }

        public void OnNotificationRemoved(OpenNotification sbn)
        {
            if (sbn.GetPackage() == "android" && sbn.GetTag() == "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay")
                return;

            if (sbn.IsSummary())
                return; //Ignore the summary notification.

            int position = GetNotificationPosition(sbn);
            OpenNotification notificationToBeRemoved = null;

            if (position >= 0)
            {
                //if found, then use the Notification to be removed instead. 
                //the reason is that the 'sbn' coming from this method has less data.
                //then it makes data that I need from the notification unavailable.
                notificationToBeRemoved = StatusBarNotifications[position];

                StatusBarNotifications.RemoveAt(position);
                using (var h = new Handler(Looper.MainLooper))
                    h.Post(() =>
                    {
                            //When removing a summary notification it causes a IndexOutOfBoundsException...
                            //notificationAdapter.NotifyItemRemoved(position);
                            //This has to be fixed, anyway, because this change makes the adapter to lose  the animations when removing a item
                            notificationAdapter.NotifyDataSetChanged();
                    });
            }
            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = !(StatusBarNotifications.Where(n => n.IsRemovable()).ToList().Count==0)
            });
            NotificationRemoved?.Invoke(this, new NotificationRemovedEventArgs()
            {
                OpenNotification = notificationToBeRemoved ?? sbn, //avoid nulls.
            });
        }

        public void CancelAllNotifications()
        {
            notificationAdapter.NotifyDataSetChanged();
        }

        private static int GetNotificationPosition(OpenNotification sbn)
        {
            return StatusBarNotifications.IndexOf(StatusBarNotifications.FirstOrDefault
                (o => o.GetId() == sbn.GetId() && o.GetPackage() == sbn.GetPackage() && o.GetTag()== sbn.GetTag() &&
            o.IsSummary() == sbn.IsSummary()));
        }

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            NotificationListSizeChanged?.Invoke(this, e);
        }

        private void OnNotificationPosted(bool shouldCauseWakeup, OpenNotification sbn, bool updatesPreviousNotification)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs()
            {
                ShouldCauseWakeUp = shouldCauseWakeup,
                OpenNotification = sbn,
                UpdatesPreviousNotification = updatesPreviousNotification
            });
        }

        public static OpenNotification GetOpenNotification(string customId)
        {
            return StatusBarNotifications.Where(o => o.GetCustomId() == customId).FirstOrDefault();
        }
    }
}