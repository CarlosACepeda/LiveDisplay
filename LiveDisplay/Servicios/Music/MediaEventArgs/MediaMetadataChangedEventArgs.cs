﻿using Android.App;
using Android.Media;
using System;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    internal class MediaMetadataChangedEventArgs : EventArgs
    {
        public MediaMetadata MediaMetadata { get; set; }
        public PendingIntent ActivityIntent { get; set; }
    }
}