using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Models
{
    class NotificationListWorker
    {
        public StatusBarNotification[] NotificationList 
        { get 
            { return statusBarNotifications; } private set { value= statusBarNotifications; } }

        private static NotificationListWorker instance;

        readonly private StatusBarNotification[] statusBarNotifications;

        private NotificationListWorker(StatusBarNotification[] statusBarNotifications)
        {
            this.statusBarNotifications = statusBarNotifications;
        }
        public static NotificationListWorker GetInstance(StatusBarNotification[] statusBarNotifications)
        {
            if (instance == null)
                instance = new NotificationListWorker(statusBarNotifications);
            return instance;
        }
    }
}