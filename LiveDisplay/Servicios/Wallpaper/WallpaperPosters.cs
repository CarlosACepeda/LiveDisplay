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
    //enumeration for a series of possible entities that can set wallpapers to the lockscreen
    public enum WallpaperPoster
    {
        None = 0, 
        Lockscreen = 1,
        MusicPlayer = 2,
        Notification = 3, //A notification can also set a wallpaper, 
        //for example, the MediaStyle and the BigPictureStyle of a notification uses the Big Picture to use it as the wallpaper.
    }
}