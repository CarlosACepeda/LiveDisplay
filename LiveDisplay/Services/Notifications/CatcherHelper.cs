using Android.App;
using Android.OS;
using Android.Service.Notification;
using LiveDisplay.Adapters;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Services.Notifications
{
    internal class CatcherHelper : Java.Lang.Object
    {
        public static NotificationAdapter NotificationAdapter { get; set; }
        public static List<OpenNotification> StatusBarNotifications { get; internal set; } = new List<OpenNotification>();

        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;
        private const string ANDROID_TAG_FOR_FLOATING_LIVEDISPLAY = "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay";
        private const string ANDROID_APP_PACKAGE = "android";

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
            NotificationAdapter = new NotificationAdapter(StatusBarNotifications);
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
            if (sbn.ApplicationPackage == ANDROID_APP_PACKAGE && sbn.Tag == ANDROID_TAG_FOR_FLOATING_LIVEDISPLAY)
                return;

            var blockingstatus = Blacklist.ReturnBlockLevel(sbn.ApplicationPackage);

            if (!blockingstatus.HasFlag(LevelsOfAppBlocking.Blacklisted))
            {
                if (!blockingstatus.HasFlag(LevelsOfAppBlocking.BlockInAppOnly))
                {
                    bool causesWakeUp;
                    if (sbn.NotificationPriority >= (int)NotificationPriority.Default) //Solves a issue where non important notifications also turn on screen.
                        //anyway this is a hotfix, a better method shoudl be used to improve the blacklist/the importance of notifications.
                        causesWakeUp = true;
                    else
                        causesWakeUp = false;

                    NotificationAdapter.InsertIntoList(sbn);
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
                    notificationSlave.CancelNotification(sbn.ApplicationPackage, sbn.Tag, sbn.Id);
                }
            }

            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = StatusBarNotifications.Count > 0
            });
        }

        public void OnNotificationRemoved(OpenNotification sbn)
        {
            if (sbn.ApplicationPackage == ANDROID_APP_PACKAGE && sbn.Tag == ANDROID_TAG_FOR_FLOATING_LIVEDISPLAY)
                return;


            NotificationAdapter.RemoveNotification(sbn);
            

            //TODO: move this to Adapter.
            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = !(StatusBarNotifications.Where(n => n.IsRemovable()).ToList().Count == 0)
            });
        }

        public void CancelAllNotifications()
        {
            NotificationAdapter.NotifyDataSetChanged();
        }

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            NotificationListSizeChanged?.Invoke(this, e);
        }

        public static OpenNotification GetOpenNotification(string customId)
        {
            return StatusBarNotifications.Where(o => o.GetCustomId() == customId).FirstOrDefault();
        }
    }
}