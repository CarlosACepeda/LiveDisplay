using LiveDisplay.Models;
using System;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class NotificationRemovedEventArgs : EventArgs
    {
        //the StatusBarNotification that was just removed
        public OpenNotification OpenNotification { get; set; }
    }
}