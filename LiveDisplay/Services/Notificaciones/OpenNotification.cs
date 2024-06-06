using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Service.Notification;
using Java.Util;
using LiveDisplay.Misc;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Services.Notifications
{
    public class OpenNotification : Java.Lang.Object
    {
        public const string BigPictureStyle = "android.app.Notification$BigPictureStyle";
        public const string InboxStyle = "android.app.Notification$InboxStyle";
        public const string MediaStyle = "android.app.Notification$MediaStyle";
        public const string MessagingStyle = "android.app.Notification$MessagingStyle"; //Only available on API Level 24 and up.
        public const string BigTextStyle = "android.app.Notification$BigTextStyle";
        public const string DecoratedCustomViewStyle = "android.app.Notification$DecoratedCustomViewStyle";
        private readonly StatusBarNotification statusbarnotification;

        public OpenNotification(StatusBarNotification sbn)
        {
            statusbarnotification = sbn;
        }

        public StatusBarNotification UnderlyingStatusBarNotification => statusbarnotification;
        public string Key
        {
            get
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
                    return statusbarnotification.Key;
                return string.Empty;
            }
        }
        public int Id => statusbarnotification.Id;

        public string Tag => statusbarnotification.Tag;

        public string PackageName => statusbarnotification.PackageName;

        public string Title => statusbarnotification.Notification.Extras.GetString(Notification.ExtraTitle);

        public string Text => statusbarnotification.Notification.Extras.GetString(Notification.ExtraText);

        public string SummaryText => statusbarnotification.Notification.Extras.GetString(Notification.ExtraSummaryText);

        public string[] TextLines => statusbarnotification.Notification.Extras.GetCharSequenceArray(Notification.ExtraTextLines);
        public string BigText => statusbarnotification.Notification.Extras.GetString(Notification.ExtraBigText);

        public string SubText => statusbarnotification.Notification.Extras.GetCharSequence(Notification.ExtraSubText);

        public List<OpenAction> Actions => statusbarnotification.Notification.Actions?.Select((x) => new OpenAction(x)).ToList(); 

        internal bool IsClearable => statusbarnotification.IsClearable;

        public bool HasActions
        {
            get
            {
                if (statusbarnotification.Notification.Actions != null)
                {
                    return true;
                }
                return false;
            }
        }

        public MediaSession.Token MediaSessionToken =>
                 statusbarnotification.Notification.Extras.Get(Notification.ExtraMediaSession) as MediaSession.Token;

        internal string When
        {
            get
            {
                if (statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraShowWhen) == true)
                {
                    Calendar calendar = Calendar.Instance;
                    calendar.TimeInMillis = statusbarnotification.Notification.When;
                    return string.Format("{0:D2}:{1:D2} {2}", calendar.Get(CalendarField.Hour), calendar.Get(CalendarField.Minute), calendar.GetDisplayName((int)CalendarField.AmPm, (int)CalendarStyle.Short, Locale.Default));
                }
                return string.Empty;
            }
        }
        public long PostTime => statusbarnotification.PostTime;

        public string AppName => PackageUtils.GetTheAppName(statusbarnotification.PackageName);

        public Icon SmallIcon
        {
            get 
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    return statusbarnotification.Notification.SmallIcon;
                }
                else
                {
                    return Icon.CreateWithResource(
                        new Application().CreatePackageContext(PackageName, PackageContextFlags.Restricted), statusbarnotification.Notification.Icon);
                }
            }
        }
        public Bitmap BigPicture=> statusbarnotification.Notification.Extras.GetParcelable(Notification.ExtraPicture, Java.Lang.Class.FromType(typeof(Bitmap))) as Bitmap;

        internal Bitmap LargeIcon
        {
            get {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                    return statusbarnotification.Notification.Extras.GetParcelable(Notification.ExtraLargeIcon, Java.Lang.Class.FromType(typeof(Bitmap))) as Bitmap;

                return statusbarnotification.Notification.LargeIcon;
            }
        }
        public PendingIntent ContentIntent => statusbarnotification.Notification.ContentIntent;
        public PendingIntent FullScreenIntent => statusbarnotification.Notification.FullScreenIntent;
        
        //internal Bitmap GetPersonAvatar()
        //{
        //    if (Style() != "android.app.Notification$MessagingStyle" || Build.VERSION.SdkInt < BuildVersionCodes.P)
        //        return null;

        //}
        internal NotificationPriority NotificationPriority=>(NotificationPriority)statusbarnotification.Notification.Priority;

        internal NotificationImportance NotificationImportance
        {
            get
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                    return (NotificationImportance)(-1);

                return NotificationImportance.Unspecified; //No way to retrieve the Notification Channel for Any app except mine.

            }
        }

        public string NotificationChannelId
        {
            get
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                    return null;

                return statusbarnotification.Notification.ChannelId;
            }

        }

        internal string Style => statusbarnotification.Notification.Extras.GetString(Notification.ExtraTemplate);

        public bool IsAutoCancellable =>statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.AutoCancel);

        public bool BelongsToGroup =>Build.VERSION.SdkInt >= BuildVersionCodes.M && statusbarnotification.IsGroup;

        public bool IsSummary => Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.GroupSummary);

        internal int Progress => statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgress);

        internal int ProgressMax => statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgressMax);

        internal bool IsProgressIndeterminate => statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraProgressIndeterminate);

        public int[] CompactViewActionsIndices=> statusbarnotification.Notification.Extras.GetIntArray(Notification.ExtraCompactActions);
        internal bool IsOngoing=> statusbarnotification.IsOngoing;

    }
}