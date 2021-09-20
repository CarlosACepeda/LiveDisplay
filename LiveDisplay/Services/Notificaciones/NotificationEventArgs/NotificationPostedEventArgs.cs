using System;
using System.Collections.Generic;

namespace LiveDisplay.Servicios.Notificaciones.NotificationEventArgs
{
    public class NotificationPostedEventArgs : EventArgs
    {
        //Field to determine if the notification should wake up the device.
        public bool ShouldCauseWakeUp { get; set; }

        //the posted notification id used to find the notification within the list.
        public int NotificationPostedId { get; set; }

        public List<OpenNotification> OpenNotifications { get; set; } //the notification that was posted including the summary notification and its siblings if applicable.

        //This notification updates a existent notification?
        public bool UpdatesPreviousNotification { get; set; }
    }
}