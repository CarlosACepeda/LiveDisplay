using System;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    internal class NotificationCancelledEventArgsKitkat : EventArgs
    {
        public string NotificationPackage { get; set; }
        public string NotificationTag { get; set; }
        public int NotificationId { get; set; }
    }
}