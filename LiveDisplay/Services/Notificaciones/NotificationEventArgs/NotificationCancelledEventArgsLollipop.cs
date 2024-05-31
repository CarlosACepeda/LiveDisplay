using System;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    internal class NotificationCancelledEventArgsLollipop : EventArgs
    {
        public string Key { get; set; }
    }
}