using Android.Graphics;
using System;

namespace LiveDisplay.Services.Music.MediaEventArgs
{
    internal class MediaMetadataChangedKitkatEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public long Duration { get; set; }
        public Bitmap AlbumArt { get; set; }
    }
}