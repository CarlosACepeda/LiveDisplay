using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class CurrentWallpaperClearedEventArgs
    {
        //Property indicating who posted a wallpaper before the current wallpaper was set.
        public WallpaperPoster PreviousWallpaperPoster { get; set; }
    }
}