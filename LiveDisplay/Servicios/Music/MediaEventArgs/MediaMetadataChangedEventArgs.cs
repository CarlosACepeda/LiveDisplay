using Android.App;
using Android.Media;
using System;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    internal class MediaMetadataChangedEventArgs : EventArgs
    {
        public MediaMetadata MediaMetadata { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
        public RemoteController.MetadataEditor MediaMetadataKitkat { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete
        public PendingIntent ActivityIntent { get; set; }
        public string AppName { get; set; }
    }
}