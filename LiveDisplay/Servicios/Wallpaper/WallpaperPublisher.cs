using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using System;
using System.Threading;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperPublisher
    {
        private static WallpaperPoster CurrentWallpaperPoster { get; set; } = WallpaperPoster.None;
        private static WallpaperPoster PreviousWallpaperPoster { get; set; }

        public static event EventHandler<CurrentWallpaperClearedEventArgs> CurrentWallpaperCleared; //This event notifies listeners that the wallpaper that was being displayed on the lockscreen has been removed.

        public static event EventHandler<WallpaperChangedEventArgs> NewWallpaperIssued; //This event notifies listeners the Wallpaper that has been issued.

                                                                                        //For now the only listener of this event is 'LockScreen'

        public static void ChangeWallpaper(WallpaperChangedEventArgs e)
        {
            //It works please refactor because im sure it can be improved:
            //maybe we can use LinkedLists to keep a queue of Wallpapers and its posters? 

            if (CurrentWallpaperPoster != e.WallpaperPoster) //This validation is because is invalid to the same entity to be at the same time the current wallpaper poster and also the previous one.
            {
                PreviousWallpaperPoster = CurrentWallpaperPoster;
                CurrentWallpaperPoster = e.WallpaperPoster;
            }

            bool alreadywaiting = false;
            //The seconds of attention Property is to indicate for how long should the Lockscreen (or any listener of NewWallpaperIssued event) show this Wallpaper.
            //After the time has passed we notify that the wallpaper is now cleared (which means that any caller can now set another wallpaper)
            if (e.SecondsOfAttention > 0)
            {
                if (alreadywaiting == false)
                    ThreadPool.QueueUserWorkItem(method =>
                    {
                        alreadywaiting = true;
                        Thread.Sleep(e.SecondsOfAttention * 1000);
                        CurrentWallpaperCleared?.Invoke(null, new CurrentWallpaperClearedEventArgs { PreviousWallpaperPoster = PreviousWallpaperPoster });
                        alreadywaiting = false;
                    }
                    );
            }
            if (e.BlurLevel >= 0 && e.BlurLevel <= 25)
            {
                if (e.Wallpaper?.Bitmap != null)
                {
                    try
                    {
                        BlurImage blurImage = new BlurImage(Application.Context);
                        blurImage.Load(e.Wallpaper.Bitmap).Intensity(e.BlurLevel);
                        e.Wallpaper = new BitmapDrawable(Application.Context.Resources, blurImage.GetImageBlur());
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

        //Callers(entities) will use this call to force a CurrentWallpaperCleared event to be fired on this class.
        //The reason is that for some reason an entity that posted a wallpaper without putting seconds of attention to automatically clear
        //the wallpaper after <n> seconds stopped or is no longer available
        //Then this wallpaper that this entity posted won't be cleared ever and the other entities will think that this entity that stopped is currently
        //posting that wallpaper so the other entities won't be capable of posting their wallpapers.
        public static void ReleaseWallpaper()
        {
            CurrentWallpaperCleared?.Invoke(null, new CurrentWallpaperClearedEventArgs { PreviousWallpaperPoster = PreviousWallpaperPoster });
        }
    }
}