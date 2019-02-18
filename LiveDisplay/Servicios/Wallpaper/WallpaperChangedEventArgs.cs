using Android.Graphics.Drawables;
using System;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperChangedEventArgs : EventArgs
    {
        public Drawable Wallpaper { get; set; }
        public short OpacityLevel { get; set; }
        public bool IsThisWallpaperTemporary { get; set; } //Field to indicate 
    }
}