using Android.App;
using Android.Graphics.Drawables;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using LiveDisplay.Models;
using LiveDisplay.Services.Music;
using LiveDisplay.Services.Wallpaper;
using System;

namespace LiveDisplay.Services.Notifications.NotificationStyle
{
    internal class MediaStyleNotification : NotificationStyle
    {
        private bool ActionsCompacted { get; set; } = false; //Default value, though, this should be linked somehow with the current status of the view, to ensure robustness.

        public MediaStyleNotification(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
      : base(openNotification, ref notificationView, notificationFragment)
        {
            var notificationMediaArtwork = new BitmapDrawable(Application.Context.Resources, OpenNotification.MediaArtwork);
            //Only post the Artwork if this notification isn't the one that keeps the Music Widget Active (because in that case it will cause redundancy, the Music Widget 
            //will be already showing the Artwork)
            if (MusicController.MediaSessionAssociatedWThisNotification(openNotification.GetCustomId()) == false)
            {
                WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                {
                    BlurLevel = 1,
                    OpacityLevel = 125,
                    SecondsOfAttention = 5,
                    Wallpaper = notificationMediaArtwork,
                    WallpaperPoster = WallpaperPoster.Notification,
                });
            }
        }

        protected override void SetWhen()
        {
            When.Text = string.Empty;
        }

        protected override void SetActionText(Button action, string text)
        {
            action.Text = string.Empty;
        }

        protected override void Collapse_Click(object sender, EventArgs e)
        {
            //It works, but I am not sure how this will improve user experience, tbh, so I commented it out.

            //int[] compactViewIndices = OpenNotification.CompactViewActionsIndices();
            ////In this case we'll try to do some of  Android's default behavior.
            ////We will try to reduce the size of the actions while fading away the ones that aren't meant to be in the collapsed view.
            //for (int i = 0; i <= NotificationActions.ChildCount-1; i++)
            //{
            //    if(compactViewIndices.Contains(i)== false)
            //    {
            //        ImageButton action = (ImageButton)NotificationActions.GetChildAt(i);

            //        action.Alpha = 0.5f;
            //        action.Enabled = false;
            //    }
            //}
        }
    }
}