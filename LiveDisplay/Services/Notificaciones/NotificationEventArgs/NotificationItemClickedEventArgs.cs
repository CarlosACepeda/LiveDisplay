using System;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class NotificationItemClickedEventArgs : EventArgs
    {
        public int Position { get; set; }
        public OpenNotification OpenNotification { get; set; }
    }
}