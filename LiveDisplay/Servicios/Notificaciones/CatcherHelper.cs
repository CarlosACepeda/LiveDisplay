using Android.OS;
using Android.Service.Notification;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios.Notificaciones
{
    /// <summary>
    /// This class is made for Help the main Catcher to do its Actions like:
    /// Update a notification.
    /// Insert a Notification.
    /// Initialize the data (?) maybe.
    /// Group a notification, this is a new feature in Nougat(API Level 24+)
    /// Also is made to allow more funcionality and keep the Catcher Service readable and understandable
    /// (for me and others, gracias al cielo)
    /// </summary>
    internal class CatcherHelper : Java.Lang.Object
    {
        public static NotificationAdapter notificationAdapter;
        public static List<StatusBarNotification> statusBarNotifications;

        public static event EventHandler NotificationRemoved;

        public static event EventHandler<NotificationPostedEventArgs> NotificationPosted; //NotifyItemInserted.

        public static event EventHandler<NotificationItemClickedEventArgs> NotificationUpdated; //NotifyItemUpdated.

#pragma warning disable CS0067 // El evento 'CatcherHelper.NotificationGrouped' nunca se usa

        public static event EventHandler NotificationGrouped; // TODO: Clueless (?) :'-( I don't know how to implement this in LockScreen

#pragma warning restore CS0067 // El evento 'CatcherHelper.NotificationGrouped' nunca se usa
#pragma warning disable CS0067 // El evento 'CatcherHelper.NotificationUngrouped' nunca se usa

        public static event EventHandler NotificationUngrouped; //TODO

#pragma warning restore CS0067 // El evento 'CatcherHelper.NotificationUngrouped' nunca se usa

        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;

        public static bool thereAreNotifications = false;

        /// <summary>
        /// Constructor of the Class
        /// </summary>
        /// <param name="statusBarNotifications">This list is sent by Catcher, and is used to fill the Adapter
        /// that the RecyclerView will use.
        /// </param>
        public CatcherHelper(List<StatusBarNotification> statusBarNotifications)
        {
            CatcherHelper.statusBarNotifications = statusBarNotifications;
            notificationAdapter = new NotificationAdapter(statusBarNotifications);
            if (statusBarNotifications.Count > 0)
            {
                thereAreNotifications = true;
            }
        }

        //If Catcher call this, it means that the notification is part of a Group of notifications and should be Grouped.
        private void GroupNotification()
        {
            //After this, fire event.
            //Find ID, if Found, Append to that notification, if not WtF. lol.
        }

        public void OnNotificationPosted(StatusBarNotification sbn)
        {
            if (!UpdateNotification(sbn))
            {
                InsertNotification(sbn);
            }

            if (statusBarNotifications.Count > 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = true
                });
                thereAreNotifications = true;
            }
        }

        private void InsertNotification(StatusBarNotification sbn)
        {
            if (Blacklist.IsAppBlacklisted(sbn.PackageName))
            {
                statusBarNotifications.Add(sbn);
                using (var h = new Handler(Looper.MainLooper))
                    h.Post(() => { notificationAdapter.NotifyItemInserted(statusBarNotifications.Count); });

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
        }

        private void OnNotificationUpdated(int position)
        {
            NotificationUpdated?.Invoke(this, new NotificationItemClickedEventArgs
            {
                Position = position
            }
                );
        }

        private bool UpdateNotification(StatusBarNotification sbn)
        {
            int indice = GetNotificationPosition(sbn);
            if (indice >= 0 && Blacklist.IsAppBlacklisted(sbn.PackageName) == false)
            {
                statusBarNotifications.RemoveAt(indice);
                statusBarNotifications.Add(sbn);
                using (var h = new Handler(Looper.MainLooper))
                    h.Post(() => { notificationAdapter.NotifyItemChanged(indice); });

                OnNotificationUpdated(indice);
                return true;
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
                return false;
            }
            
        }

        private void RemoveNotificationFromGroup()
        {
            //After this, fire event.
        }

        public void OnNotificationRemoved(StatusBarNotification sbn)
        {
            int position = GetNotificationPosition(sbn);
            if (position >= 0)
            {
                statusBarNotifications.RemoveAt(position);
                using (var h = new Handler(Looper.MainLooper))
                    h.Post(() => { notificationAdapter.NotifyItemRemoved(position); });
            }
            //Check if when removing this notification the list size is zero, if true, then raise an event that will
            //indicate the lockscreen to hide the 'Clear all button'
            if (statusBarNotifications.Count == 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = false
                });
                thereAreNotifications = false;
            }
        }

        public void CancelAllNotifications()
        {
            notificationAdapter.NotifyDataSetChanged();
        }

        private int GetNotificationPosition(StatusBarNotification sbn)
        {
            int index = statusBarNotifications.IndexOf(statusBarNotifications.FirstOrDefault(o => o.Id == sbn.Id && o.PackageName == sbn.PackageName));

            return index;
        }

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            NotificationListSizeChanged?.Invoke(this, e);
        }

        private void OnNotificationPosted()
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs()
            {
                ShouldCauseWakeUp = true //Implementing blacklist...
            });
        }

        private void OnNotificationRemoved()
        {
            NotificationRemoved?.Invoke(this, EventArgs.Empty); //Just notify the subscribers the notification was removed.
        }
    }
}