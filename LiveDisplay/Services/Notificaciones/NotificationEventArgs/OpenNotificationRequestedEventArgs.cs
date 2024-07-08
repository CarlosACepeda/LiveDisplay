using System;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class OpenNotificationRequestedEventArgs: EventArgs
    {
        public Func<OpenNotification, bool> Predicate;
    }
}