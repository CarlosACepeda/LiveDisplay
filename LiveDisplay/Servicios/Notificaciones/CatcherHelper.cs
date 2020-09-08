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
        public static List<StatusBarNotification> StatusBarNotifications { get; internal set; }

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
            StatusBarNotifications = statusBarNotifications;
            notificationAdapter = new NotificationAdapter(statusBarNotifications);
            if (statusBarNotifications.Count > 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = true
                });
            }
        }

        public void OnNotificationPosted(StatusBarNotification sbn)
        {
            if (sbn == null) { return; }
            //This is the notification of 'LiveDisplay is showing above other apps'
            //Simply let's ignore it, because it's annoying. (Anyway, the user couldn't care less about this notification tbh)
            if (sbn.PackageName == "android" && sbn.Tag == "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay")
                return;

            if (new OpenNotification(sbn).IsSummary())
                return; //Ignore the summary notification. (it causes redundancy) anyway, In an ideal scenario I should hide this notification instead
            //of ignoring it.

            var blockingstatus = Blacklist.ReturnBlockLevel(sbn.PackageName);

            if (!blockingstatus.HasFlag(LevelsOfAppBlocking.Blacklisted))
            {
                if (!blockingstatus.HasFlag(LevelsOfAppBlocking.BlockInAppOnly))
                {
                    bool causesWakeUp = false;
                    if (sbn.Notification.Priority >= (int)NotificationPriority.Default) //Solves a issue where non important notifications also turn on screen.
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
                    notificationSlave.CancelNotification(sbn.Key);
                }
                else
                {
                    notificationSlave.CancelNotification(sbn.PackageName, sbn.Tag, sbn.Id);
                }
            }


            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = StatusBarNotifications.Count > 0
            }) ;
            
        }

        public void OnNotificationRemoved(StatusBarNotification sbn)
        {
            if (sbn.PackageName == "android" && sbn.Tag == "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay")
                return;

            if (new OpenNotification(sbn).IsSummary())
                return; //Ignore the summary notification.

            int position = GetNotificationPosition(sbn);
            OpenNotification notificationToBeRemoved = null;

            if (position >= 0)
            {
                //if found, then use the Notification to be removed instead. 
                //the reason is that the 'sbn' coming from this method has less data.
                //then it makes data that I need from the notification unavailable.
                notificationToBeRemoved = new OpenNotification(StatusBarNotifications[position]);

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
                ThereAreNotifications = !(StatusBarNotifications.Where(n => n.IsClearable).ToList().Count==0)
            });
            NotificationRemoved?.Invoke(this, new NotificationRemovedEventArgs()
            {
                OpenNotification = notificationToBeRemoved ?? new OpenNotification(sbn), //avoid nulls.
            });
        }

        public void CancelAllNotifications()
        {
            notificationAdapter.NotifyDataSetChanged();
        }

        private int GetNotificationPosition(StatusBarNotification sbn)
        {
            return StatusBarNotifications.IndexOf(StatusBarNotifications.FirstOrDefault
                (o => o.Id == sbn.Id && o.PackageName == sbn.PackageName && o.Tag== sbn.Tag &&
            o.Notification.Flags.HasFlag(NotificationFlags.GroupSummary) == sbn.Notification.Flags.HasFlag(NotificationFlags.GroupSummary)));
        }

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            NotificationListSizeChanged?.Invoke(this, e);
        }

        private void OnNotificationPosted(bool shouldCauseWakeup, StatusBarNotification sbn, bool updatesPreviousNotification)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs()
            {
                ShouldCauseWakeUp = shouldCauseWakeup,
                OpenNotification = new OpenNotification(sbn),
                UpdatesPreviousNotification = updatesPreviousNotification
            });
        }

        
    }
}