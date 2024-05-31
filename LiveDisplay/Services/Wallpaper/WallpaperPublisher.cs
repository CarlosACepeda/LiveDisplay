using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LiveDisplay.Services.Wallpaper
{
    public class WallpaperPublisher
    {
        private const int infiniteSeconds = 0;

        private static readonly List<WallpaperChangedEventArgs> wallpaperPosters = new List<WallpaperChangedEventArgs>();

        public static event EventHandler<WallpaperChangedEventArgs> NewWallpaperIssued; //This event notifies listeners the Wallpaper that has been issued

        public static event EventHandler<EventArgs> OnZeroPublishersAvailable;

        private static Timer timeoutTimer;
        private static WallpaperChangedEventArgs temporalWallpaperPoster;

        public static void ChangeWallpaper(WallpaperChangedEventArgs e)
        {
            var incomingWallpaperPoster = e;
            var existentWallpaperPoster = wallpaperPosters.FirstOrDefault(w => w.WallpaperPoster == incomingWallpaperPoster.WallpaperPoster);
            bool posterDoesNotExist =  existentWallpaperPoster == null;
            if (posterDoesNotExist)
            {
                wallpaperPosters.Add(incomingWallpaperPoster);
            }
            else if (existentWallpaperPoster.SecondsOfAttention> infiniteSeconds && incomingWallpaperPoster.SecondsOfAttention> infiniteSeconds)
            {
                //It means that this entity wants to post a new temporal wallpaper without finishing the previous temporal wallpaper
                Console.WriteLine($"{incomingWallpaperPoster.WallpaperPoster} is posting a new TEMPORAL wallpaper without finishing previous TEMPORAL wallpaper, duration {incomingWallpaperPoster.SecondsOfAttention} seconds");

                wallpaperPosters.Remove(existentWallpaperPoster); //let's remove the old one then.
                timeoutTimer.Stop(); //Prevent the execution of the temporal wallpaper timeout, as we aren't needing it anymore.

            }
            Console.WriteLine(
                $"{incomingWallpaperPoster.WallpaperPoster} is posting a {(incomingWallpaperPoster.SecondsOfAttention > infiniteSeconds ? "Temporal" : "Permanent")} wallpaper, duration {incomingWallpaperPoster.SecondsOfAttention} seconds");

            if (incomingWallpaperPoster.SecondsOfAttention>infiniteSeconds)
            {
                temporalWallpaperPoster = incomingWallpaperPoster;
                wallpaperPosters.Add(incomingWallpaperPoster);
                StartTimeout(incomingWallpaperPoster.SecondsOfAttention * 1000);
            }
            if (incomingWallpaperPoster.BlurLevel >= 0 && incomingWallpaperPoster.BlurLevel <= 25)
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
            if (incomingWallpaperPoster.OpacityLevel >= 0 || incomingWallpaperPoster.OpacityLevel <= 255)
            {
                if (incomingWallpaperPoster.Wallpaper?.Bitmap != null)
                {
                    incomingWallpaperPoster.Wallpaper.Alpha = incomingWallpaperPoster.OpacityLevel;
                }
            }

            NewWallpaperIssued?.Invoke(null, incomingWallpaperPoster);

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
            else
            {
                OnZeroPublishersAvailable?.Invoke(null, null);
            }
            timeoutTimer.Stop();
            timeoutTimer.Elapsed -= TimeoutTimer_Elapsed;
        }
    }
}