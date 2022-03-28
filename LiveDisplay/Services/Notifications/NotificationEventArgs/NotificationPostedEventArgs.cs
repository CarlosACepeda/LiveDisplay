using LiveDisplay.Models;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class NotificationPostedEventArgs : EventArgs
    {
        //Field to determine if the notification should wake up the device.
        public bool ShouldCauseWakeUp { get; set; }

        public OpenNotification NotificationPosted { get; set; }

        //This notification updates a existent notification?
        public bool UpdatesPreviousNotification { get; set; }
    }
}