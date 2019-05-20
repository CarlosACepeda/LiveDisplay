using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Service.Notification;
using Android.Util;
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
                Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                calendar.TimeInMillis = statusbarnotification.Notification.When;
                return string.Concat(calendar.Get(Java.Util.CalendarField.Hour), ":", calendar.Get(Java.Util.CalendarField.Minute));
            }
            catch
            {
                return "";
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

        public Drawable GetActionIcon()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                return IconFactory.ReturnActionIconDrawable(action.Icon, action.ActionIntent.CreatorPackage);
            }
            return null;
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

        public string GetPlaceholderTextForInlineResponse()
        {
            return action.GetRemoteInputs()[1].Label;
        }

        public void Dispose()
        {
            action.Dispose();
        }
    }
}