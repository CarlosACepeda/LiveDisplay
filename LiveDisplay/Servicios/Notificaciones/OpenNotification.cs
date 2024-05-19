using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Util;
using Java.Util;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios.Notificaciones
{
    public class OpenNotification : Java.Lang.Object, PendingIntent.IOnFinished
    {
        public const string BigPictureStyle = "android.app.Notification$BigPictureStyle";
        public const string InboxStyle = "android.app.Notification$InboxStyle";
        public const string MediaStyle = "android.app.Notification$MediaStyle";
        public const string MessagingStyle = "android.app.Notification$MessagingStyle"; //Only available on API Level 24 and up.
        public const string BigTextStyle = "android.app.Notification$BigTextStyle";
        public const string DecoratedCustomViewStyle = "android.app.Notification$DecoratedCustomViewStyle";
        private StatusBarNotification statusbarnotification;

        public OpenNotification(StatusBarNotification sbn)
        {
            statusbarnotification = sbn;
            try
            {
                //var context = Application.Context.CreatePackageContext(sbn.PackageName, PackageContextFlags.Restricted);
                //notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public StatusBarNotification GetUnderlyingStatusBarNotification()
        {
            return statusbarnotification;
        }
        public string GetKey()
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
                return statusbarnotification.Key;

            return string.Empty;
        }

        public int GetId()
        {
            return statusbarnotification.Id;
        }

        public void Cancel()
        {
            if (IsClearable())
                using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
                {
                    if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    {
                        slave.CancelNotification(GetPackageName(), GetTag(), GetId());
                    }
                    else
                    {
                        slave.CancelNotification(GetKey());
                    }
                }
        }

        public string GetTag() => statusbarnotification.Tag;

        public string GetPackageName() => statusbarnotification.PackageName;

        public string Title()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetString(Notification.ExtraTitle);
            }
            catch
            {
                return "";
            }
        }

        public string Text()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetString(Notification.ExtraText);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetSummaryText()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetString(Notification.ExtraSummaryText);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetTextLines()
        {
            try
            {
                string textlinesformatted = string.Empty;
                var textLines = statusbarnotification.Notification.Extras.GetCharSequenceArray(Notification.ExtraTextLines);
                foreach (var line in textLines)
                {
                    textlinesformatted = textlinesformatted + line + " \n"; //Add new line.
                }
                return textlinesformatted;
            }
            catch
            {
                return null;
            }
        }

        public string GetBigText()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetString(Notification.ExtraBigText);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string SubText()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetCharSequence(Notification.ExtraSubText).ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public void ClickNotification()
        {
            try
            {
                var intent = statusbarnotification.Notification.ContentIntent;
                intent ??= statusbarnotification.Notification.FullScreenIntent;

                //This is part of a Workaround to make LockScreen show on Android Q devices and above:
                //Please check CatcherHelper#OnNotificationPosted() to get an idea of how it works.

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q && GetPackageName() == "com.underground.livedisplay" /*Only act on notifications sent by this app*/)
                {
                    //Causes a FullScreenIntent that's contained within a Notification matchig the if statement to be sent correctly.
                    //For some unknown reason the usual "Send()" method doesn't work if the screen is locked.
                    intent.Send(Result.Ok, this, new Handler());
                    Cancel(); //ignoring documentation: if we leave this notification alive after performing the previous line intent.Send(...),
                              //then after if the same notification gets posted without the previous one being removed then the intent.Send(...) won't succeed.
                              //and the lockscreen won't show.
                              //Android is weird.
                }
                intent.Send();
                //Android Docs: For NotificationListeners: When implementing a custom click for notification
                //Cancel the notification after it was clicked when this notification is autocancellable.
                if (IsAutoCancellable())
                    Cancel();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Click Notification failed, fail in pending intent {ex.Message}");
            }
        }

        public List<OpenAction> RetrieveActions()
        {
            return statusbarnotification.Notification.Actions?.Select((x) => new OpenAction(x)).ToList();
        }

        internal bool IsClearable()
        {
            return statusbarnotification.IsClearable;
        }

        public bool HasActions()
        {
            if (statusbarnotification.Notification.Actions != null)
            {
                return true;
            }
            return false;
        }

        public MediaSession.Token GetMediaSessionToken()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetParcelable(
                    Notification.ExtraMediaSession, Java.Lang.Class.FromType(typeof(MediaSession.Token))) as MediaSession.Token;
            }
            catch
            {
                return null;
            }
        }

        public bool RepresentsMediaPlaying()
        {
            var mediaSessionToken = GetMediaSessionToken();
            return mediaSessionToken != null;
        }

        internal string When()
        {
            try
            {
                if (statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraShowWhen) == true)
                {
                    Calendar calendar = Calendar.Instance;
                    calendar.TimeInMillis = statusbarnotification.Notification.When;
                    return string.Format("{0:D2}:{1:D2} {2}", calendar.Get(CalendarField.Hour), calendar.Get(CalendarField.Minute), calendar.GetDisplayName((int)CalendarField.AmPm, (int)CalendarStyle.Short, Locale.Default));
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        internal long PostTime()
        {
            return statusbarnotification.PostTime;
        }

        internal string AppName()
        {
            try
            {
                return PackageUtils.GetTheAppName(statusbarnotification.PackageName);
            }
            catch
            {
                return string.Empty;
            }
        }

        internal Icon GetSmallIcon()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                return statusbarnotification.Notification.SmallIcon;
            }
            else
            {
                return Icon.CreateWithResource(
                    new Application().CreatePackageContext(GetPackageName(), PackageContextFlags.Restricted), statusbarnotification.Notification.Icon);
            }
        }
        internal Bitmap BigPicture()
        {
            return statusbarnotification.Notification.Extras.GetParcelable(Notification.ExtraPicture, Java.Lang.Class.FromType(typeof(Bitmap))) as Bitmap;
        }

        internal Bitmap MediaArtwork()
        {
            if(Build.VERSION.SdkInt< BuildVersionCodes.O)
                return statusbarnotification.Notification.Extras.GetParcelable(Notification.ExtraLargeIcon, Java.Lang.Class.FromType(typeof(Bitmap))) as Bitmap;

            return statusbarnotification.Notification.LargeIcon;
        }
        //internal Bitmap GetPersonAvatar()
        //{
        //    if (Style() != "android.app.Notification$MessagingStyle" || Build.VERSION.SdkInt < BuildVersionCodes.P)
        //        return null;

        //}
        internal NotificationPriority GetNotificationPriority()
        {
            try
            {
                return (NotificationPriority)statusbarnotification.Notification.Priority;
            }
            catch
            {
                return (NotificationPriority)(-155);
            }
        }

        internal NotificationImportance GetNotificationImportance()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return (NotificationImportance)(-1);

            return  NotificationImportance.Unspecified; //No way to retrieve the Notification Channel for Any app except mine.
        }

        public string GetNotificationChannelId()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return null;

            return  statusbarnotification.Notification.ChannelId;
        }

        internal string Style()
        {
            return statusbarnotification.Notification.Extras.GetString(Notification.ExtraTemplate);
        }

        public bool IsAutoCancellable()
        {
            return statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.AutoCancel);
        }

        //<test only, check if this notification is part of a group or is a group summary or any info related with group notifications.>
        internal string GetGroupInfo()
        {
            string result = "";
            if (statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.GroupSummary) == true)
            {
                result += " This is summary!";
            }
            else
            {
                result += " This is NOT summary!";
            }

            if (Style() != null)
                result = result + "The Style is+ " + Style();
            else
                result += " It does not have Style!";

            if (statusbarnotification.IsGroup)
                result += " Is Group";
            else
                result += " Is not group";

            result += "\n" + "Package: " + GetPackageName() + " Id: " + GetId() + " Tag :" + GetTag()
                + " Importance: " + GetNotificationImportance() + " Priority: " + GetNotificationPriority();
            return result;
        }

        public bool BelongsToGroup()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.N) return false;
            else return statusbarnotification.IsGroup;
        }

        public bool IsSummary()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.Kitkat) return false;
            else return statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.GroupSummary);
        }

        internal int GetProgress()
        {
            return statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgress);
        }

        internal int GetProgressMax()
        {
            return statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgressMax);
        }

        internal bool IsProgressIndeterminate()
        {
            return statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraProgressIndeterminate);
        }

        public int[] CompactViewActionsIndices()
        {
            return statusbarnotification.Notification.Extras.GetIntArray(Notification.ExtraCompactActions);
        }
        internal bool IsOnGoing()
        {
            return statusbarnotification.IsOngoing;
        }

        public void OnSendFinished(PendingIntent pendingIntent, Intent intent, [GeneratedEnum] Result resultCode, string resultData, Bundle resultExtras)
        {
            Console.WriteLine($"Android Q background activity launch was defeated by me (debug info, FullScreenIntent result):  {resultCode} || {resultData}");
        }
    }

    public class OpenAction : Java.Lang.Object
    {
        private Notification.Action action;
        private RemoteInput remoteInputDirectReply;
        private RemoteInput[] remoteInputs;

        public OpenAction(Notification.Action action)
        {
            this.action = action;
        }

        public string Title()
        {
            return action.Title.ToString();
        }

        public void ClickAction()
        {
            try
            {
                action.ActionIntent.Send();
            }
            catch
            {
                Log.Info("LiveDisplay", "Click notification action failed");
            }
        }

        public bool ActionRepresentDirectReply()
        {
            //Direct reply action is a new feature in Nougat, so when called on Marshmallow and backwards, so in those cases an Action will never represent a Direct Reply.
            if (Build.VERSION.SdkInt <= BuildVersionCodes.M) return false;

            remoteInputs = action.GetRemoteInputs();
            if (remoteInputs == null || remoteInputs?.Length == 0) return false;

            //In order to consider an action representing a Direct Reply we check for the ResultKey of that remote input.
            foreach (var remoteInput in remoteInputs)
            {
                if (remoteInput.ResultKey != null)
                {
                    remoteInputDirectReply = remoteInput;
                    return true;
                }
            }
            return false;
        }

        public Drawable GetActionIcon()
        {
            Drawable actionIcon;
            try
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
                {
                    actionIcon= IconFactory.ReturnActionIconDrawable(action.Icon, action.ActionIntent.CreatorPackage);
                }
                else
                {
                    actionIcon = IconFactory.ReturnActionIconDrawable(action.JniPeerMembers.InstanceFields.GetInt32Value("icon.I", action), action.ActionIntent.CreatorPackage);
                }
            }
            catch
            {
                return null;
            }
            return actionIcon;
        }

        public string GetPlaceholderTextForInlineResponse()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.M) return string.Empty;

            return remoteInputDirectReply.Label;
            
        }

        public bool SendInlineResponse(string responseText)
        {
            try
            {
                Bundle bundle = new Bundle();
                Intent intent = new Intent();
                bundle.PutCharSequence(remoteInputDirectReply.ResultKey, responseText);
                RemoteInput.AddResultsToIntent(remoteInputs, intent, bundle);
                action.ActionIntent.Send(Application.Context, Result.Ok, intent);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}