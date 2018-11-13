using System;

namespace LiveDisplay.Servicios.Notificaciones.NotificationEventArgs
{
    internal class NotificationCancelledEventArgsLollipop : EventArgs
    {
        public string Key { get; set; }
    }
}