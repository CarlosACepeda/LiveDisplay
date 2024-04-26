using System;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class NotificationListSizeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Boolean to check if the Notification list has notifications
        /// </summary>
        public bool ThereAreNotifications { get; set; }
    }
}