using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperPublisher
    {
        private const int infiniteSeconds = 0;

        private static readonly List<WallpaperChangedEventArgs> wallpaperPosters = new List<WallpaperChangedEventArgs>();

        public static event EventHandler<WallpaperChangedEventArgs> NewWallpaperIssued; //This event notifies listeners the Wallpaper that has been issued.

                                                                                        //For now the only listener of this event is 'LockScreen'
        private static Timer timeoutTimer;
        private static WallpaperChangedEventArgs temporalWallpaperPoster;

        public static void ChangeWallpaper(WallpaperChangedEventArgs e)
        {
            if (wallpaperPosters.FirstOrDefault(w => w.WallpaperPoster == e.WallpaperPoster) == null)
            {
                wallpaperPosters.Add(e);
            }
            Console.WriteLine($"{e.WallpaperPoster} is posting a {(e.SecondsOfAttention > infiniteSeconds ? "Temporal" : "Permanent")} wallpaper, duration {e.SecondsOfAttention} seconds");

            if (e.SecondsOfAttention>infiniteSeconds)
            {
                temporalWallpaperPoster = e;

                StartTimeout(e.SecondsOfAttention * 1000);
               
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

        private static void StartTimeout(int interval)
        {
            timeoutTimer = new Timer
            {
                Interval = interval,
                AutoReset = false
            };
            timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            timeoutTimer.Start();
        }

        private static void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"{temporalWallpaperPoster.WallpaperPoster} is being removed because it was temporary, duration {temporalWallpaperPoster.SecondsOfAttention} seconds");
            wallpaperPosters.Remove(temporalWallpaperPoster);
            if (wallpaperPosters.Count >= 1)
            {
                Console.WriteLine($"posting previous wallpaper from {wallpaperPosters[^1].WallpaperPoster}");
                NewWallpaperIssued?.Invoke(null, wallpaperPosters[^1]); //after removing this temporal wallpaper, issue the previous wallpaper, if available.
            }
            timeoutTimer.Stop();
        }
    }
}