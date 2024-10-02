using System;
using System.Collections.Generic;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class RequestedOpenNotificationGeneratedEventArgs : EventArgs
    {
        public List<OpenNotification> OpenNotifications { get; set; }
        public int RequestCode { get; set; } = 0;
    }
}
