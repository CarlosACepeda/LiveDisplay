using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Service.Notification;
using Android.Util;
using Java.Util;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Music;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios.Notificaciones
{
    internal class OpenNotification : Java.Lang.Object
    {
        private StatusBarNotification statusbarnotification;
        private MediaController mediaController; //A media controller to be used with the  Media Session token provided by a MediaStyle notification.
        public OpenNotification(StatusBarNotification sbn)
        {
            statusbarnotification = sbn;
        }
        private string GetKey()
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
                return statusbarnotification.Key;

            return string.Empty;
        }
        private int GetId()
        {
            return statusbarnotification.Id;
        }
        public void Cancel()
        {
            if (IsRemovable())
                using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
                {
                    //If this notification has a mediacontroller callback registered we unregister it, to avoid leaks.
                    if (mediaController != null)
                    {
                        mediaController.UnregisterCallback(MusicController.GetInstance());
                    }
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
        //I need to pinpoint this notification, this is the way.
        //When-> Helps to really ensure is the same notification by checking also the time it was posted
        public string GetCustomId() => GetPackageName() + GetTag() + GetId()+ When();
        private string GetTag() => statusbarnotification.Tag;
        private string GetPackageName() => statusbarnotification.PackageName;
        public string Title()
        {
            try
            {
                return statusbarnotification.Notification.Extras.Get(Notification.ExtraTitle).ToString();
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
                return statusbarnotification.Notification.Extras.Get(Notification.ExtraText).ToString();
            }
            catch
            {
                return "";
            }
        }

        public string GetSummaryText()
        {
            try
            {
                return statusbarnotification.Notification.Extras.Get(Notification.ExtraSummaryText).ToString();
            }
            catch
            {
                return "";
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
                return statusbarnotification.Notification.Extras.Get(Notification.ExtraBigText).ToString();
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
                statusbarnotification.Notification.ContentIntent.Send();
                //Android Docs: For NotificationListeners: When implementing a custom click for notification
                //Cancel the notification after it was clicked when this notification is autocancellable.
                Cancel();
            }
            catch
            {
                Console.WriteLine("Click Notification failed, fail in pending intent");
            }
        }

        public List<Notification.Action> RetrieveActions()
        {
            return statusbarnotification.Notification.Actions?.ToList();
        }

        internal bool IsRemovable()
        {
            if (statusbarnotification.IsClearable == true)
            {
                return true;
            }
            return false;
        }

        public bool HasActions()
        {
            if (statusbarnotification.Notification.Actions != null)
            {
                return true;
            }
            return false;
        }
        private MediaSession.Token GetMediaSessionToken()
        {
            try
            {
                return statusbarnotification.Notification.Extras.Get(Notification.ExtraMediaSession) as MediaSession.Token;
            }
            catch
            {
                return null;
            }
        }
        public bool StartMediaCallback()
        {
            var mediaSessionToken = GetMediaSessionToken();
            if (mediaSessionToken == null) return false;
            else
            {
                try
                {
                    if (mediaController != null)
                    {
                        mediaController.UnregisterCallback(MusicController.GetInstance());
                    }
                    mediaController = new MediaController(Application.Context, GetMediaSessionToken());
                    var musicController = MusicController.GetInstance();
                    mediaController.RegisterCallback(MusicController.GetInstance());
                    musicController.TransportControls = mediaController.GetTransportControls();
                    musicController.MediaMetadata = mediaController.Metadata;
                    musicController.PlaybackState = mediaController.PlaybackState;
                    musicController.ActivityIntent = statusbarnotification.Notification.ContentIntent; //The current notification.
                    Log.Info("LiveDisplay", "Callback registered Successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Info("LiveDisplay", "Callback failed Successfully: LOL" + ex.Message);
                    return false;
                }
            }
        }

        internal string When()
        {
            try
            {
                if (statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraShowWhen) == true)
                {
                    Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
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

        internal string AppName()
        {
            try
            {
                return PackageUtils.GetTheAppName(statusbarnotification.PackageName);
            }
            catch
            {
                return "";
            }
        }

        internal Bitmap BigPicture()
        {
            return statusbarnotification.Notification.Extras.Get(Notification.ExtraPicture) as Bitmap;
        }
        internal Bitmap MediaArtwork()
        {
            return statusbarnotification.Notification.Extras.Get(Notification.ExtraLargeIcon) as Bitmap;
        }

        internal NotificationPriority GetNotificationPriority()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return (NotificationPriority)statusbarnotification.Notification.Priority;

            return (NotificationPriority)(-1);
        }

        internal NotificationImportance GetNotificationImportance()
        {
            //TODO
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return (NotificationImportance)(-1);

            return (NotificationImportance)(-1);

        }
        private NotificationChannel GetNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return null;

            return null; //TODO.
        }
        internal string Style()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetString(Notification.ExtraTemplate);
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool IsAutoCancellable()
        {
            if (statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.AutoCancel) == true)
            {
                return true;
            }
            return false;
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

            result += "\n" + "Package: "+ GetPackageName()+ " Id: "+ GetId()+ " Tag :" + GetTag();

            return result;
        }
        internal int GetProgress()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgress);
            }
            catch
            {
                return -2;
            }
        }
        internal int GetProgressMax()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgressMax);
            }
            catch
            {
                return -2;
            }
        }
        internal bool IsProgressIndeterminate()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraProgressIndeterminate);
            }
            catch
            {
                return false;
            }
        }
    }

    internal class OpenAction: Java.Lang.Object
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
            try
            {
                return action.Title.ToString();
            }
            catch
            {
                return "";
            }
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
            if (Build.VERSION.SdkInt < BuildVersionCodes.N) return false;
            
            remoteInputs = action.GetRemoteInputs();
            if (remoteInputs == null || remoteInputs?.Length == 0) return false;

            //In order to consider an action who represents a Direct Reply we check for the ResultKey of that remote input.
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
            try
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
                {
                    return IconFactory.ReturnActionIconDrawable(action.Icon, action.ActionIntent.CreatorPackage);
                }
                else
                {
                    return IconFactory.ReturnActionIconDrawable(action.JniPeerMembers.InstanceFields.GetInt32Value("icon.I", action), action.ActionIntent.CreatorPackage);
                }
            }
            catch
            {
                return null;
            }
        }

        public string GetPlaceholderTextForInlineResponse()
        {
            //Direct reply action is a new feature in Nougat, so this method call is invalid in Marshmallow and backwards, let's return empty.
            if (Build.VERSION.SdkInt < BuildVersionCodes.N) return string.Empty;

            try
            {
                return remoteInputDirectReply.Label;
            }
            catch
            {
                return string.Empty;
            }
        }

        //Since API 24 Nougat.
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