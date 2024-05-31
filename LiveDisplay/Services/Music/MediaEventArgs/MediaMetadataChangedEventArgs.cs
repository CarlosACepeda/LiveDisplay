using Android.App;
using Android.Media;
using System;

namespace LiveDisplay.Services.Media.MediaEventArgs
{
    internal class MediaMetadataChangedEventArgs : EventArgs
    {
        public MediaMetadata MediaMetadata { get; set; }
        public RemoteController.MetadataEditor MediaMetadataKitkat { get; set; }
        public PendingIntent ActivityIntent { get; set; }
        public string AppName { get; set; }
    }
}