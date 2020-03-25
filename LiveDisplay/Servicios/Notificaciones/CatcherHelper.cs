using Android.App;
using Android.OS;
using Android.Service.Notification;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios.Awake;
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

        public static event EventHandler NotificationRemoved;

        public static event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;

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
            AwakeHelper awakeHelper = new AwakeHelper(); //Will help us to make certain operations as waking up the screen and such.
        }

        //If Catcher call this, it means that the notification is part of a Group of notifications and should be Grouped.
        private void GroupNotification()
        {
            //After this, fire event.
            //Find ID, if Found, Append to that notification, if not WtF. lol.
        }

        public void OnNotificationPosted(StatusBarNotification sbn)
        {
            //This is the notification of 'LiveDisplay is showing above other apps'
            //Simply let's ignore it, because it's annoying. (Anyway, the user couldn't care less about this notification tbh)
            if (sbn.PackageName == "android" && sbn.Tag == "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay")
                return;

            var blockingstatus = Blacklist.ReturnBlockLevel(sbn.PackageName);

            if (!blockingstatus.HasFlag(LevelsOfAppBlocking.Blacklisted))
            {
                if (!blockingstatus.HasFlag(LevelsOfAppBlocking.BlockInAppOnly))
                {
                    int index = GetNotificationPosition(sbn); //Tries to get the index of a possible already existing notification in the list of notif.
                    if (index >= 0)
                    {
                        //It exists within the list.
                        //SO it should be updated.

                        StatusBarNotifications.RemoveAt(index);
                        StatusBarNotifications.Add(sbn);
                        using (var h = new Handler(Looper.MainLooper))
                            h.Post(() => { notificationAdapter.NotifyItemChanged(index); });
                        OnNotificationPosted(blockingstatus.HasFlag(LevelsOfAppBlocking.None), sbn, true);
                    }
                    else
                    {
                        StatusBarNotifications.Add(sbn);

                        using (var h = new Handler(Looper.MainLooper))
                            h.Post(() => { notificationAdapter.NotifyItemInserted(StatusBarNotifications.Count); });
                        OnNotificationPosted(blockingstatus.HasFlag(LevelsOfAppBlocking.None), sbn, false);
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

            if (StatusBarNotifications.Count > 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = true
                });
            }
        }

        public void OnNotificationRemoved(StatusBarNotification sbn)
        {
            int position = GetNotificationPosition(sbn);
            if (position >= 0)
            {
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

            if (StatusBarNotifications.Count == 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = false
                });
            }
            OnNotificationRemoved();
        }

        public void CancelAllNotifications()
        {
            notificationAdapter.NotifyDataSetChanged();
        }

        private int GetNotificationPosition(StatusBarNotification sbn)
        {
            return StatusBarNotifications.IndexOf(StatusBarNotifications.FirstOrDefault(o => o.Id == sbn.Id && o.PackageName == sbn.PackageName &&
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
                StatusBarNotification = sbn,
                UpdatesPreviousNotification = updatesPreviousNotification
            });
        }

        private void OnNotificationRemoved()
        {
            NotificationRemoved?.Invoke(this, EventArgs.Empty); //Just notify the subscribers the notification was removed.
        }
    }
}