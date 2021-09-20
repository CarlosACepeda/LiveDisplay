using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Misc
{
    public class Constants
    {
        public const string CLOCK_FRAGMENT = "clock";
        public const string NOTIFICATION_FRAGMENT = "notification";
        public const string MUSIC_FRAGMENT = "music";

        public const string BIG_PICTURE_STYLE = "android.app.Notification$BigPictureStyle";
        public const string INBOX_STYLE = "android.app.Notification$InboxStyle";
        public const string MEDIA_STYLE = "android.app.Notification$MediaStyle";
        public const string MESSAGING_STYLE = "android.app.Notification$MessagingStyle"; //Only available on API Level 24 and up.
        public const string BIG_TEXT_STYLE = "android.app.Notification$BigTextStyle";
        public const string DECORATED_CUSTOM_VIEW_STYLE = "android.app.Notification$DecoratedCustomViewStyle";

    }
}