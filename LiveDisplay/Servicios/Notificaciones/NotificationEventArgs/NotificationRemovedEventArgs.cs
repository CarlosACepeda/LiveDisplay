using System;

namespace LiveDisplay.Servicios.Notificaciones.NotificationEventArgs
{
    internal class NotificationRemovedEventArgs : EventArgs
    {
        //the StatusBarNotification that was just removed
        public OpenNotification OpenNotification { get; set; }
    }
}