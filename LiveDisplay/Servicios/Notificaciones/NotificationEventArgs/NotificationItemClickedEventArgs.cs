using System;

namespace LiveDisplay.Servicios.Notificaciones.NotificationEventArgs
{
    public class NotificationItemClickedEventArgs : EventArgs
    {
        public int Position { get; set; }
    }
}