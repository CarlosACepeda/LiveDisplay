using Android.Graphics.Drawables;
using System;
using Android.Content;
using Android.Graphics;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperChangedEventArgs : EventArgs
    {
        public BitmapDrawable Wallpaper { get; set; }
        public short OpacityLevel { get; set; }
        public short BlurLevel { get; set; }

        //Property indicating for how many seconds should the Lockscreen show this Wallpaper, after that, it will
        //reload the previous one.
        public short SecondsOfAttention { get; set; }
    }
}