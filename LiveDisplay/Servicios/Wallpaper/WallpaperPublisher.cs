using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperPublisher
    {
        private const int infiniteSeconds = 0;

        private static List<WallpaperChangedEventArgs> wallpaperPosters = new List<WallpaperChangedEventArgs>();

        public static event EventHandler<WallpaperChangedEventArgs> NewWallpaperIssued; //This event notifies listeners the Wallpaper that has been issued.

                                                                                        //For now the only listener of this event is 'LockScreen'

        public static void ChangeWallpaper(WallpaperChangedEventArgs e)
        {
            if (wallpaperPosters.FirstOrDefault(w => w.WallpaperPoster == e.WallpaperPoster) == null)
            {
                wallpaperPosters.Add(e);
            }
            Console.WriteLine($"{e.WallpaperPoster} is posting a {(e.SecondsOfAttention > infiniteSeconds ? "Temporal" : "Permanent")} wallpaper, duration {e.SecondsOfAttention * 1000} seconds");

            if (e.SecondsOfAttention>infiniteSeconds)
            {
                var temporalWallpaperPoster = e;

                using (var timeoutTimer = new System.Timers.Timer{ Interval= e.SecondsOfAttention * 1000, AutoReset= false})
                {
                    timeoutTimer.Start();
                    timeoutTimer.Elapsed += (sender, e) => 
                    {
                        Console.WriteLine($"{temporalWallpaperPoster.WallpaperPoster} is being removed because it was temporary, duration {temporalWallpaperPoster.SecondsOfAttention} seconds");
                        wallpaperPosters.Remove(temporalWallpaperPoster);
                        if (wallpaperPosters.Count >= 1)
                        {
                            Console.WriteLine($"posting previous wallpaper from {wallpaperPosters[^1].WallpaperPoster}");
                            NewWallpaperIssued?.Invoke(null, wallpaperPosters[^1]); //after removing this temporal wallpaper, issue the previous wallpaper, if available.
                        }
                    };
                }
            }


            if (e.BlurLevel >= 0 && e.BlurLevel <= 25)
            {
                if (e.Wallpaper?.Bitmap != null)
                {
                    try
                    {
                        
                    }
                    catch (Exception ex)
                    {
                        Log.Error("LiveDisplay", "Failed to blur wallpaper: " + ex.Message);
                    }
                }
            }
            if (e.OpacityLevel >= 0 || e.OpacityLevel <= 255)
            {
                if (e.Wallpaper?.Bitmap != null)
                {
                    e.Wallpaper.Alpha = e.OpacityLevel;
                }
            }
            NewWallpaperIssued?.Invoke(null, e);
        }

    }
}