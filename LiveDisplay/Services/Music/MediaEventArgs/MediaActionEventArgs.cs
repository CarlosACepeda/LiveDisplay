using Android.Media.Session;
using LiveDisplay.Misc;
using System;

namespace LiveDisplay.Services.Media.MediaEventArgs
{
    public class MediaActionEventArgs : EventArgs
    {
        public MediaActionFlags MediaActionFlags { get; set; }
        public long Time { get; set; }

        public PlaybackState.CustomAction CustomAction { get; set; }
        public OpenAction CompactedAction { get; set; } //Acts as the custom action when CustomAction is not available.
    }
}