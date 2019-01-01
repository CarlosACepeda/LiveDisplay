using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperChangedEventArgs: EventArgs
    {
        public Drawable Wallpaper { get; set; }
        public short OpacityLevel { get; set; }
    }
}