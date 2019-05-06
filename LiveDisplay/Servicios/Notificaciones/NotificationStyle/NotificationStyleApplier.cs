using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using LiveDisplay.Servicios.Wallpaper;
using System.Threading;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    /// <summary>
    /// Self explaining, this will apply the notification styles.
    /// But, I will interpret them differently as android does, to be adapted to my lockscreen.
    /// </summary>
    internal class NotificationStyleApplier : Java.Lang.Object
    {
        private const string BigPictureStyle = "android.app.Notification$BigPictureStyle";
        private const string InboxStyle = "android.app.Notification$InboxStyle";
        private const string MediaStyle = "android.app.Notification$MediaStyle";
        private const string MessagingStyle = "android.app.Notification$MessagingStyle";
        private Resources resources;
        private OpenNotification openNotification;

        public NotificationStyleApplier(View notificationView, OpenNotification openNotification)
        {
            this.openNotification = openNotification;
        }

        public void ApplyStyle(string which)
        {
            switch (which)
            {
                case BigPictureStyle:
                    ThreadPool.QueueUserWorkItem(method =>
                    {
                        var notificationBigPicture = new BitmapDrawable(openNotification.BigPicture());
                        WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = notificationBigPicture, OpacityLevel = 125, SecondsOfAttention = 5 });
                    });
                    break;

                case InboxStyle:
                    break;

                case MediaStyle:
                    break;

                case MessagingStyle:
                    break;
            }
        }
    }
}