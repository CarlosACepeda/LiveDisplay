using Android.App;
using Android.Media;
using Android.Net.Wifi.Aware;
using LiveDisplay.Servicios.Notificaciones;
using System;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    internal class MediaMetadataChangedEventArgs : EventArgs
    {
        public string OpenNotificationId { get; set; } //The openNotification id that is related to this MediaMetadata.
        public MediaMetadata MediaMetadata { get; set; }
        public PendingIntent ActivityIntent { get; set; }
        public string AppName { get; set; }
    }
}