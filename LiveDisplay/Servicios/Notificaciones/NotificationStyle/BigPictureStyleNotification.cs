using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    public class BigPictureStyleNotification: NotificationStyle
    {
        public BigPictureStyleNotification(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
    :   base(openNotification, ref notificationView, notificationFragment)
        {
            var notificationBigPicture = new BitmapDrawable(Resources, OpenNotification.BigPicture());
            WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
            {
                BlurLevel = 1,
                OpacityLevel = 125,
                SecondsOfAttention = 5,
                Wallpaper = notificationBigPicture,
                WallpaperPoster = WallpaperPoster.Notification,
            });
        }

    }
}