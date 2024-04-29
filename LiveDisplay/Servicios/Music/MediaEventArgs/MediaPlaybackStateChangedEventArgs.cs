using Android.Media;
using Android.Media.Session;
using System;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    public class MediaPlaybackStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Argument indicating the current playback state of the media, playing, stopped, etc-
        /// </summary>
        public PlaybackStateCode PlaybackState { get; set; }

        public RemoteControlPlayState PlaybackStateKitkat { get; set; }

        public long CurrentTime { get; set; }
    }
}