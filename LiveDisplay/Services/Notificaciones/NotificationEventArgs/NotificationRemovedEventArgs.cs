using System;
namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    class NotificationRemovedEventArgs: EventArgs
    {
        //the StatusBarNotification that was just removed
        public OpenNotification OpenNotification { get; set; }


    }
}