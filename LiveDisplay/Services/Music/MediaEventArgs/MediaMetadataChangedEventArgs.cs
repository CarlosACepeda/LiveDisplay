using Android.App;
using Android.Graphics;
using Android.Media;
using System;

namespace LiveDisplay.Services.Media.MediaEventArgs
{
    internal class MediaMetadataChangedEventArgs : EventArgs
    {
        public string MediaTitle { get; set; }

        public string MediaArtist { get; set; }

        public string MediaAlbum { get; set; }

        public long MediaDuration { get; set; }
        public Bitmap MediaArtwork { get; set; }

        public PendingIntent ActivityIntent { get; set; }
        public string AppName { get; set; }
    }
}