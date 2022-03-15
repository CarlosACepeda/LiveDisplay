using Android.App;
using Android.OS;
using Android.Service.Notification;
using LiveDisplay.Adapters;
using LiveDisplay.Models;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Services.Notifications
{
    internal class NotificationHijackerWorker : Java.Lang.Object
    {
        public static NotificationAdapter NotificationAdapter { get; set; }
        public static List<OpenNotification> StatusBarNotifications { get; internal set; } = new List<OpenNotification>();

        private const string ANDROID_TAG_FOR_FLOATING_LIVEDISPLAY = "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay";
        private const string ANDROID_APP_PACKAGE = "android";
        private static NotificationHijackerWorker instance;

        /// <summary>
        /// Constructor of the Class
        /// </summary>
        /// <param name="statusBarNotifications">This list is sent by Catcher, and is used to fill the Adapter
        /// that the RecyclerView will use, it is tighly coupled with that adapter.
        /// </param>

        public static NotificationHijackerWorker GetInstance(List<StatusBarNotification> statusBarNotifications, bool recreate)
        {
            if (instance == null || recreate)
                instance = new NotificationHijackerWorker(statusBarNotifications);
            return instance;
        }
        private NotificationHijackerWorker(List<StatusBarNotification> statusBarNotifications)
        {
            StatusBarNotifications = new List<OpenNotification>();
            foreach (var sbn in statusBarNotifications)
            {
                StatusBarNotifications.Add(new OpenNotification(sbn));
            }
            NotificationAdapter = new NotificationAdapter(StatusBarNotifications);
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
        }

        public void OnNotificationRemoved(OpenNotification sbn)
        {
            if (sbn.ApplicationPackage == ANDROID_APP_PACKAGE && sbn.Tag == ANDROID_TAG_FOR_FLOATING_LIVEDISPLAY)
                return;


            NotificationAdapter.RemoveNotification(sbn);
            
        }

        public void CancelAllNotifications()
        {
            NotificationAdapter.NotifyDataSetChanged();
        }
        public static OpenNotification GetOpenNotification(string customId)
        {
            return StatusBarNotifications.FirstOrDefault(o => o.GetCustomId == customId);
        }
        public static bool DeviceSupportsNotificationGrouping()
        {
            return (Build.VERSION.SdkInt >= BuildVersionCodes.N);
        }
        bool CanReadNotificationPriority()
        {
            return (Build.VERSION.SdkInt <= BuildVersionCodes.N); //Nougat and inferior versions can.
        }
        public static void RemoveNotification(OpenNotification openNotification)
        {
            //There's a really strange bug where removed Incoming Calls notifications never gets to OnNotificationRemoved method on the NotificationListener
            //so the notification stays within our list. TODO: Find a way to remove a notification when this happens.

            if (openNotification.IsRemovable)
                using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
                {
                    if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    {
                        slave.CancelNotification(openNotification.PackageName, openNotification.Tag, openNotification.Id);
                    }
                    else
                    {
                        slave.CancelNotification(openNotification.Key);
                    }
                }
        }
        public static void ClickNotification(OpenNotification openNotification)
        {
            try
            {
                openNotification.ContentIntent.Send();
                //Android Docs: For NotificationListeners: When implementing a custom click for notification
                //Cancel the notification after it was clicked when this notification is autocancellable.
                RemoveNotification(openNotification);
            }
            catch
            {
                Console.WriteLine("Click Notification failed, fail in pending intent");
            }
        }
    }
}