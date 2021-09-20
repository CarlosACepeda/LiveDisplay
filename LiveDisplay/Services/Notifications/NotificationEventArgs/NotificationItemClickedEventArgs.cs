using System;
using System.Collections.Generic;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class NotificationItemClickedEventArgs : EventArgs
    {
        public int Position { get; set; }
        public OpenNotification StatusBarNotification { get; set; }
        public List<OpenNotification> ChildNotifications { get; set; }
    }
}