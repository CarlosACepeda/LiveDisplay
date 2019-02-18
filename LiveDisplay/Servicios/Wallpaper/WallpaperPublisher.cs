using System;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class WallpaperPublisher
    {
        public static event EventHandler<WallpaperChangedEventArgs> WallpaperChanged;

        public static void OnWallpaperChanged(WallpaperChangedEventArgs e)
        {
            WallpaperChanged?.Invoke(null, e);
        }
    }
}