﻿using Android.Graphics.Drawables;
using Android.Widget;
using LiveDisplay.Models;
using LiveDisplay.Services.Wallpaper;

namespace LiveDisplay.Services.Notifications.NotificationStyle
{
    public class BigPictureStyleNotification : NotificationStyle
    {
        public BigPictureStyleNotification(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
    : base(openNotification, ref notificationView, notificationFragment)
        {
            var notificationBigPicture = new BitmapDrawable(Resources, OpenNotification.BigPicture);
            WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
            {
                BlurLevel = 5,
                OpacityLevel = 125,
                SecondsOfAttention = 5,
                Wallpaper = notificationBigPicture,
                WallpaperPoster = WallpaperPoster.Notification,
            });
        }
    }
}