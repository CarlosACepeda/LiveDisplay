using Android.App;
using Android.Media;
using Android.Net.Wifi.Aware;
using System;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    internal class MediaMetadataChangedEventArgs : EventArgs
    {
        public MediaMetadata MediaMetadata { get; set; }
        public PendingIntent ActivityIntent { get; set; }
        public string AppName { get; set; }
    }
}