using Android.Media;
using System;

namespace LiveDisplay.Services.Music.MediaEventArgs
{
    internal class MediaPlaybackStateChangedKitkatEventArgs : EventArgs
    {
        public RemoteControlPlayState PlaybackState { get; set; }
        public long CurrentTime { get; set; }
    }
}