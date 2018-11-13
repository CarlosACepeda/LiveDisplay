using LiveDisplay.Misc;
using System;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    internal class MediaActionEventArgs : EventArgs
    {
        public MediaActionFlags MediaActionFlags { get; set; }
        public long Time { get; set; }
    }
}