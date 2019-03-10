using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
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

        public OpenNotification(int position)
        {
            this.position = position;
        }

        public string GetTitle()
        {
            try
            {
                return CatcherHelper.statusBarNotifications[position].Notification.Extras.Get(Notification.ExtraTitle).ToString();
            }
            catch
            {
                return "";
            }
        }

        public string GetText()
        {
            try
            {
                return CatcherHelper.statusBarNotifications[position].Notification.Extras.Get(Notification.ExtraText).ToString();
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
                CatcherHelper.statusBarNotifications[position].Notification.ContentIntent.Send();
                //Android Docs: For NotificationListeners: When implementing a custom click for notification
                //Cancel the notification after it was clicked when this notification is autocancellable.
                if (IsRemovable())
                {
                    using (NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance())
                    {
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                        {
                            int notiId = CatcherHelper.statusBarNotifications[position].Id;
                            string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
                            string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
                            notificationSlave.CancelNotification(notiPack, notiTag, notiId);
                        }
                        else
                        {
                            notificationSlave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
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
            return CatcherHelper.statusBarNotifications[position].Notification.Actions.ToList();
        }

        internal bool IsRemovable()
        {
            if (CatcherHelper.statusBarNotifications[position].IsClearable == true)
            {
                return true;
            }
            return false;
        }

        public bool NotificationHasActionButtons()
        {
            if (CatcherHelper.statusBarNotifications[position].Notification.Actions != null)
            {
                return true;
            }
            return false;
        }

        internal string GetWhen()
        {
            try
            {
                var timeinmillis = CatcherHelper.statusBarNotifications[position].Notification.When.ToString();
                DateTime dateTime = new DateTime(Convert.ToInt64(timeinmillis));
                if (dateTime.Hour == 0 && dateTime.Minute == 0)
                {
                    return "";
                }
                return dateTime.Hour + ":" + dateTime.Minute;
            }
            catch
            {
                return "";
            }
        }

        internal string GetAppName()
        {
            try
            {
                return PackageUtils.GetTheAppName(CatcherHelper.statusBarNotifications[position].PackageName);
            }
            catch
            {
                return "";
            }
        }

        internal Bitmap GetBigPicture()
        {
            return CatcherHelper.statusBarNotifications[position].Notification.Extras.Get(Notification.ExtraPicture) as Bitmap;
        }

        public static bool NotificationIsAutoCancel(int position)
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
                Log.Info("LiveDisplay", "Click notification failed");
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