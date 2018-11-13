using System;

namespace LiveDisplay.Servicios.Notificaciones.NotificationEventArgs
{
    internal class NotificationListSizeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Boolean to check if the Notification list has notifications
        /// </summary>
        public bool ThereAreNotifications { get; set; }
    }
}