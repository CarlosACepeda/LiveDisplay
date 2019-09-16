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
        Weather= 4 //The weather also posts wallpapers, depending on a certain weather condition the lockscreen can change it's wallpaper 
            //when the Weather is clicked on the lockscreen, for example, if the current weather is sunny, the wallpaper can change to a image of a sun and clear sky.
            //This premise may not happen at all but it's my first idea.
            //(TODO) actually this is not happening at all, ha ha.

    }
}