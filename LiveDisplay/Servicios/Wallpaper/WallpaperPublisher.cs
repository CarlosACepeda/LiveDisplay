using System;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperPublisher
    {
        public static event EventHandler<WallpaperChangedEventArgs> NewWallpaperIssued;

        public static void ChangeWallpaper(WallpaperChangedEventArgs e)
        {
            NewWallpaperIssued?.Invoke(null, e);
        }
    }
}