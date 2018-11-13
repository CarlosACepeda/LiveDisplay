using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    class MediaPlaybackStateChangedEventArgs: EventArgs
    {
        /// <summary>
        /// Argument indicating the current playback state of the media, playing, stopped, etc-
        /// </summary>
        public PlaybackStateCode PlaybackState { get; set; }
        public long CurrentTime { get; set; }

    }
}