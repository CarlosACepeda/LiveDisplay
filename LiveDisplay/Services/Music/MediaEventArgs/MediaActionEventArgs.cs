using LiveDisplay.Misc;
using System;

namespace LiveDisplay.Services.Media.MediaEventArgs
{
    public class MediaActionEventArgs : EventArgs
    {
        public MediaActionFlags MediaActionFlags { get; set; }
        public long Time { get; set; }
    }
}