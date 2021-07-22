﻿using System;
using System.Collections.Generic;

namespace LiveDisplay.Servicios.Notificaciones.NotificationEventArgs
{
    public class NotificationPostedEventArgs : EventArgs
    {
        //Field to determine if the notification should wake up the device.
        public bool ShouldCauseWakeUp { get; set; }

        //the StatusBarNotification that was just posted
        public OpenNotification OpenNotification { get; set; }

        //This notification updates a existent notification?
        public bool UpdatesPreviousNotification { get; set; }
    }
}