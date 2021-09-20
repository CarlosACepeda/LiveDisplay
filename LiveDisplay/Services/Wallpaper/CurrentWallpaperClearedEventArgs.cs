namespace LiveDisplay.Servicios.Wallpaper
{
    public class CurrentWallpaperClearedEventArgs
    {
        //Property indicating who posted a wallpaper before the current wallpaper was set.
        public WallpaperPoster PreviousWallpaperPoster { get; set; }
    }
}