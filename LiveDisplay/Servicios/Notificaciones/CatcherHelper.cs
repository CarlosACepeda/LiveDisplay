﻿using Android.App;
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

        public static event EventHandler NotificationRemoved;

        public static event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        public static event EventHandler<NotificationItemClickedEventArgs> NotificationUpdated;

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
            StatusBarNotifications = statusBarNotifications;
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
            //Before inserting let's check if this notification represents the summary of a group of notifications.
            //If not then check if the notification is part of a group, if is then ignore it, because we already have the summary notification. (temporary, the objective
            //eventually is to make the lockscreen capable of showing a group of notifications, so we can show all the notifications that belong to a group instead of just showing
            //the notification that represents the summary of all the notifications that belong to that group.

            //only valid since Lollipop, let's see how will i do this for Kitkat.
            //Other apps are capable of doing it why I not. 

            //if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            //{
            //    if (!sbn.Notification.Flags.HasFlag(NotificationFlags.GroupSummary))
            //    {
            //        if (statusBarNotifications.Where(no => no.GroupKey == sbn.GroupKey).FirstOrDefault() == null)
            //        {
                        if (!UpdateNotification(sbn))
                        {
                            InsertNotification(sbn);
                        }
            //        }
            //    }
            //}

            

            if (StatusBarNotifications.Count > 0)
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
            var blockingstatus = Blacklist.ReturnBlockLevel(sbn.PackageName);

            if (!blockingstatus.HasFlag(LevelsOfAppBlocking.Blacklisted))
            {
                if (!blockingstatus.HasFlag(LevelsOfAppBlocking.BlockInAppOnly))
                {
                    StatusBarNotifications.Add(sbn);

                    using (var h = new Handler(Looper.MainLooper))
                        h.Post(() => { notificationAdapter.NotifyItemInserted(StatusBarNotifications.Count); });
                    OnNotificationPosted(blockingstatus.HasFlag(LevelsOfAppBlocking.None));
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
        }

        private void OnNotificationUpdated(int position)
        {
            NotificationUpdated?.Invoke(this, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification= StatusBarNotifications[position]

            }
            );
        }

        private bool UpdateNotification(StatusBarNotification sbn)
        {
            int indice = GetNotificationPosition(sbn);
            if (indice >= 0)
            {
                StatusBarNotifications.RemoveAt(indice);
                StatusBarNotifications.Add(sbn);
                using (var h = new Handler(Looper.MainLooper))
                    h.Post(() => { notificationAdapter.NotifyItemChanged(indice); });

                OnNotificationUpdated(indice);
                return true;
            }
            return false;
        }

        public void OnNotificationRemoved(StatusBarNotification sbn)
        {
            int position = GetNotificationPosition(sbn);
            if (position >= 0)
            {
                StatusBarNotifications.RemoveAt(position);
                using (var h = new Handler(Looper.MainLooper))
                    h.Post(() => { notificationAdapter.NotifyItemRemoved(position); });
            }

            if (StatusBarNotifications.Count == 0)
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
            return StatusBarNotifications.IndexOf(StatusBarNotifications.FirstOrDefault(o => o.Id == sbn.Id && o.PackageName == sbn.PackageName));
        }

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            NotificationListSizeChanged?.Invoke(this, e);
        }

        private void OnNotificationPosted(bool shouldCauseWakeup)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs()
            {
                ShouldCauseWakeUp = shouldCauseWakeup
            });
        }

        private void OnNotificationRemoved()
        {
            NotificationRemoved?.Invoke(this, EventArgs.Empty); //Just notify the subscribers the notification was removed.
        }
    }
}