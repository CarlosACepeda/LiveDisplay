using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
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
    internal class OpenNotification : IDisposable
    {
        private int position;
        private StatusBarNotification statusbarnotification;

        public OpenNotification(int position)
        {
            this.position = position;
            statusbarnotification = CatcherHelper.statusBarNotifications[position];
        }

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
        public string[] GetTextLines()
        {
            try
            {
                return statusbarnotification.Notification.Extras.GetCharSequenceArray(Notification.ExtraTextLines);
            }
            catch
            {
                return null;
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
                return null;
            }
        }
        public void ClickNotification()
        {
            try
            {
                statusbarnotification.Notification.ContentIntent.Send();
                //Android Docs: For NotificationListeners: When implementing a custom click for notification
                //Cancel the notification after it was clicked when this notification is autocancellable.
                if (IsRemovable())
                {
                    using (NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance())
                    {
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                        {
                            int notiId = statusbarnotification.Id;
                            string notiTag = statusbarnotification.Tag;
                            string notiPack = statusbarnotification.PackageName;
                            notificationSlave.CancelNotification(notiPack, notiTag, notiId);
                        }
                        else
                        {
                            notificationSlave.CancelNotification(statusbarnotification.Key);
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Click Notification failed, fail in pending intent");
            }
        }

        public List<Notification.Action> RetrieveActions()
        {
            return statusbarnotification.Notification.Actions.ToList();
        }

        internal bool IsRemovable()
        {
            if (statusbarnotification.IsClearable == true)
            {
                return true;
            }
            return false;
        }

        public bool HasActionButtons()
        {
            if (statusbarnotification.Notification.Actions != null)
            {
                return true;
            }
            return false;
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

        public static bool IsAutoCancellable(int position)
        {
            if (CatcherHelper.statusBarNotifications[position].Notification.Flags.HasFlag(NotificationFlags.AutoCancel) == true)
            {
                return true;
            }
            return false;
        }

        public static void SendInlineText(string text)
        {
            //Implement me.
        }

        public void Dispose()
        {
            position = -1;
            statusbarnotification = null;
        }
    }

    internal class OpenAction : IDisposable
    {
        private Notification.Action action;

        public OpenAction(Notification.Action action)
        {
            this.action = action;
        }

        public string GetTitle()
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
            var remoteInputs = action.GetRemoteInputs();
            if (remoteInputs == null || remoteInputs?.Length == 0) return false;

            //In order to consider an action who represents a Direct Reply we check for the ResultKey of that remote input.
            foreach (var remoteInput in remoteInputs)
            {
                if (remoteInput.ResultKey != null)
                    return true;
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

        private void GetRemoteInput()
        {
            RemoteInput remoteInput;
            foreach (var item in action.GetRemoteInputs())
            {
                if (item.ResultKey != null)
                {
                    remoteInput = item;
                    break;
                }
            }
        }
        private void GetRemoteInput(StatusBarNotification sbn)
        {
            Intent intent = new Intent();
            Bundle bundle = new Bundle();
            RemoteInput remoteInput;
            if (sbn.Notification.Actions != null)
                foreach (var item in sbn.Notification.Actions)
                {
                    RemoteInput[] remoteInputs;
                    if (item.GetRemoteInputs() != null)
                    {
                        remoteInputs = item.GetRemoteInputs();
                        foreach (var remoteinput in remoteInputs)
                        {
                            if (remoteinput.ResultKey != null)
                            {
                                remoteInput = remoteinput;
                                bundle.PutCharSequence(remoteinput.ResultKey, string.Empty);

                                RemoteInput.AddResultsToIntent(remoteInputs, intent, bundle);

                                //remoteInput.Extras.PutCharSequence(remoteinput.ResultKey, ":)");
                                //item.Extras.PutCharSequence(remoteinput.ResultKey, ":)");
                                item.ActionIntent.Send(Application.Context, Result.Ok, intent);
                                var i = item.ActionIntent;

                                break;
                            }
                        }
                    }
                }
        }


        public string GetPlaceholderTextForInlineResponse()
        {
            return action.GetRemoteInputs()[1].Label;
        }

        public bool SendInlineResponse(string responseText)
        {
            return true;
        }

        public void Dispose()
        {
            action.Dispose();
        }
    }
}