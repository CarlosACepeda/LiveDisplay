using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Service.Notification;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.Enums;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using MediaController = Android.Media.Session.MediaController;

namespace LiveDisplay.Models
{
    public class OpenNotification : Java.Lang.Object
    {
        private StatusBarNotification statusbarnotification;
        private MediaController mediaController; //A media controller to be used with the  Media Session token provided by a MediaStyle notification.
        public const int UNKNOWN_IMPORTANCE_OR_PRIORITY = -1;
        public const int UNKNOWN = -1;

        public OpenNotification(StatusBarNotification sbn)
        {
            statusbarnotification = sbn;
        }
        public string Key { get
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
                    return statusbarnotification.Key;

                return string.Empty;
            }
        }


        public int Id => statusbarnotification.Id;

        //I need to pinpoint this notification, this is the way.
        //When-> Helps to really ensure is the same notification by checking also the time it was posted
        public string GetCustomId {
            get{ 
                if( Build.VERSION.SdkInt>= BuildVersionCodes.Lollipop)
                {
                    return statusbarnotification.Key;
                }
                else
                {
                    return statusbarnotification.Id.ToString();
                }
            }
        }

        public string Tag => statusbarnotification.Tag;

        public string PackageName => statusbarnotification.PackageName;

        public OpenMessage[] Messages
        {
            get {
                OpenMessage[] messages = null;
                //Only Valid in MessagingStyle
                if (Style == NotificationStyles.MESSAGING_STYLE)
                {
                    var messageBundles = statusbarnotification.Notification.Extras?.GetParcelableArray(NotificationBundleKeys.MESSAGE_BUNDLES);
                    if (messageBundles?.Length > 0)
                    {
                        messages = new OpenMessage[messageBundles.Length];

                        for (int i = 0; i < messageBundles.Length; i++)
                        {
                            Person sender_person = ((Bundle)messageBundles[i]).Get(NotificationBundleKeys.MESSAGE_SENDER_PERSON) as Person;

                            OpenMessage message = new OpenMessage
                            {
                                Sender = ((Bundle)messageBundles[i]).GetString(NotificationBundleKeys.MESSAGE_SENDER),
                                SenderPerson = new OpenPerson
                                {
                                    Icon = sender_person.Icon,
                                    IsBot = sender_person.IsBot,
                                    IsImportant = sender_person.IsImportant,
                                    Key = sender_person.Key,
                                    Name = sender_person.Name,
                                    NameFormatted = sender_person.NameFormatted
                                },
                                Text = ((Bundle)messageBundles[i]).GetString(NotificationBundleKeys.MESSAGE_TEXT),
                                Time = ((Bundle)messageBundles[i]).GetLong(NotificationBundleKeys.MESSAGE_TIME)
                            };
                            messages[i] = message;
                        }
                    }
                }
                return messages;
            }
        }

        public string Title
        {
            get
            {
                try
                {
                    return statusbarnotification.Notification.Extras.Get(Notification.ExtraTitle).ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        public string Text
        {
            get
            {
                try
                {
                    return statusbarnotification.Notification.Extras.Get(Notification.ExtraText).ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        public string SummaryText{
            get
            {
                try
                {
                    return statusbarnotification.Notification.Extras.Get(Notification.ExtraSummaryText).ToString();
                }
                catch { return string.Empty; }
            }
        }
        public string TextLines
        {
            get
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
        }

        public string BigText
        {
            get
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
        }

        public string SubText
        {
            get
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
        }

        public List<Notification.Action> Actions => statusbarnotification.Notification.Actions?.ToList();
        

        internal bool IsRemovable
        {
            get{
                return statusbarnotification.IsClearable;
            }
        }
        internal PendingIntent ContentIntent
        {
            get
            {
                return statusbarnotification.Notification.ContentIntent;
            }
        }
        public bool HasBeenSeenByUser { get; set; }

        public bool HasActions()
        {
            return statusbarnotification.Notification.Actions != null;
            
        }

        public MediaSession.Token MediaSessionToken
        {
            get
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
        }

        public bool RepresentsMediaPlaying
        {
            get
            {
                return MediaSessionToken != null;
            }
        }

        public bool ShowWhen
        {
            get
            {
                try
                {
                    return statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraShowWhen);
                }
                catch
                {
                    return false;
                }
            }
        }

        public string When
        {
            get
            {
                try
                {
                    if (ShowWhen)
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
        }

        public string ApplicationOwnerName
        {
            get
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
        }
        public string GroupKey
        {
            get
            {
                try
                {
                    return statusbarnotification.GroupKey;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public Bitmap BigPicture=> statusbarnotification.Notification.Extras.Get(Notification.ExtraPicture) as Bitmap;

        public Bitmap MediaArtwork
        {
           get{
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                    return statusbarnotification.Notification.Extras.Get(Notification.ExtraLargeIcon) as Bitmap;
                return statusbarnotification.Notification.LargeIcon;
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
            }
        }

        //internal Bitmap GetPersonAvatar()
        //{
        //    if (Style() != "android.app.Notification$MessagingStyle" || Build.VERSION.SdkInt < BuildVersionCodes.P)
        //        return null;

        //}
        public NotificationPriority NotificationPriority
        {
            get
            {
                try
                {
                    return (NotificationPriority)statusbarnotification.Notification.Priority;
                }
                catch
                {
                    return (NotificationPriority)UNKNOWN_IMPORTANCE_OR_PRIORITY;
                }
            }
        }

        public NotificationImportance NotificationImportance
        {
            get
            {
                //TODO (It depends on the notification channel this notification belongs to)
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                    return (NotificationImportance)(UNKNOWN_IMPORTANCE_OR_PRIORITY);

                return (NotificationImportance)(UNKNOWN_IMPORTANCE_OR_PRIORITY);
            }
        }

        public NotificationChannel NotificationChannel
        {
            get
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                    return null;

                return null;
            }//TODO: Android broke the way to retrieve notification channels, there's a way to retrieve them, but,
            //I have to be the NotificationAssistantService and that right is reserved to system apps only. (and it is too much overhead to be that
            //Assistant, as I have to provide (among other stuff) the Smart suggestions in MessagingStyle notifications.
            //On the other hand I can Register a Device Companion Service and in that way I can retrieve them, so,
            //The task is to see if I can register a dummy device to such service 
            //and trick Android into thinking I have a device and thus, the right to get the notification channels.
        }

        public RemoteViews CustomView
        {
            get
            {
                if(Style == NotificationStyles.DECORATED_CUSTOM_VIEW_STYLE)
                {
                    return statusbarnotification.Notification.ContentView;
                }
                return null;
            }
        }

        public string Style
        {
            get
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
        }

        public bool IsAutoCancellable
        {
            get
            {
                if (statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.AutoCancel))
                {
                    return true;
                }
                return false;
            }
        }

        //<test only, check if this notification is part of a group or is a group summary or any info related with group notifications.>
        internal string GetGroupInfo()
        {
            string result = "";
            if (statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.GroupSummary))
            {
                result += " This is summary!";
            }
            else
            {
                result += " This is NOT summary!";
            }

            if (Style != null)
                result = result + "The Style is+ " + Style;
            else
                result += " It does not have Style!";

            if(Build.VERSION.SdkInt>= BuildVersionCodes.N)
            if (statusbarnotification.IsGroup)
                result += " Is Group";
            else
                result += " Is not group";

            result += "\n" + "Package: " + PackageName + " Id: " + Id + " Tag :" + Tag
                + " Importance: " + NotificationImportance + " Priority: " + NotificationPriority;
            return result;
        }

        public bool BelongsToGroup
        {
            get
            {
                if (Build.VERSION.SdkInt <= BuildVersionCodes.N) return false;
                else return statusbarnotification.IsGroup;
            }
        }

        public bool IsSummary
        {
            get
            {
                if (Build.VERSION.SdkInt <= BuildVersionCodes.Kitkat) return false;
                else return statusbarnotification.Notification.Flags.HasFlag(NotificationFlags.GroupSummary);
            }
        }
        public bool IsStandalone
        {
            get
            {
                return !IsSummary && !BelongsToGroup;
            }
        }

        public int Progress
        {
            get
            {
                try
                {
                    return statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgress);
                }
                catch
                {
                    return UNKNOWN;
                }
            }
        }

        public int MaximumProgress
        {
            get
            {
                try
                {
                    return statusbarnotification.Notification.Extras.GetInt(Notification.ExtraProgressMax);
                }
                catch
                {
                    return UNKNOWN;
                }
            }
        }

        public bool ProgressIndeterminate
        {
            get
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

        public int[] CompactViewActionsIndices
        {
            get{
                return statusbarnotification.Notification.Extras.GetIntArray(Notification.ExtraCompactActions);
            }
        }

        public Icon SmallIcon
        {
            get
            {
                try
                {
                    return statusbarnotification.Notification.SmallIcon;
                }
                catch
                {
                    return null;
                }
            }
        }

        public int IconResourceInt
        {
            get
            {
                return statusbarnotification.Notification.Icon;
            }
        }

        public string ApplicationPackage =>
             statusbarnotification.PackageName;
        

        private NotificationRelevance GetRelevance()
        {
            return NotificationRelevance.DefaultRelevance;
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
            //var test1 = action.Extras;
            //var test2 = action.Extras.KeySet();
        }

        public string Title()
        {
            try
            {
                return action.Title.ToString();
            }
            catch
            {
                return string.Empty;
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

        public bool ActionRepresentsDirectReply()
        {
            //Direct reply action is a new feature in Nougat, so when called on Marshmallow and backwards, so in those cases an Action will never represent a Direct Reply.
            if (Build.VERSION.SdkInt < BuildVersionCodes.N) return false;

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

        public Drawable GetActionIcon(Color color)
        {
            Drawable actionIcon;
            try
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
                {
                    actionIcon = IconFactory.ReturnActionIconDrawable(action.Icon, action.ActionIntent.CreatorPackage);
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
            
            actionIcon.SetColorFilter(color, PorterDuff.Mode.Multiply);

            return actionIcon;
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