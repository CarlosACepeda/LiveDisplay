using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Servicios.Notificaciones
{
    internal class OpenNotification : Java.Lang.Object
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

        public static void ClickNotification(int position)
        {
            
            try
            {
                CatcherHelper.statusBarNotifications[position].Notification.ContentIntent.Send();
            }
            catch
            {
                Console.WriteLine("Click Notification failed, fail in pending intent");
            }
        }

        public static List<Button> RetrieveActions(int position)
        {
            var actions = CatcherHelper.statusBarNotifications[position].Notification.Actions;
            if (actions != null)
            {
                var buttons = new List<Button>();
                float weight = (float)1 / actions.Count;

                string paquete = CatcherHelper.statusBarNotifications[position].PackageName;
                foreach (var action in actions)
                {
                    Button anActionButton = new Button(Application.Context)
                    {
                        LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                        Text = action.Title.ToString(),
                    };
                    anActionButton.SetTypeface(Typeface.Create("sans-serif-condensed", TypefaceStyle.Normal), TypefaceStyle.Normal);
                    anActionButton.SetMaxLines(1);
                    anActionButton.SetTextColor(Color.White);
                    anActionButton.Click += (o, e) =>
                    {
                        try
                        {
                            action.ActionIntent.Send();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Action button ex:", ex.ToString());
                        }
                    };

                    anActionButton.Gravity = GravityFlags.CenterVertical;
                    TypedValue outValue = new TypedValue();
                    Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                    anActionButton.SetBackgroundResource(outValue.ResourceId);
                    if (Build.VERSION.SdkInt > BuildVersionCodes.M)
                    {
                        anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(IconFactory.ReturnActionIconDrawable(action.Icon, paquete), null, null, null);
                    }
                    buttons.Add(anActionButton);
                }
                return buttons;
            }
            return null;
        }

        internal static bool IsRemovable(int position)
        {
            if (CatcherHelper.statusBarNotifications[position].IsClearable == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool NotificationHasActionButtons(int position)
        {
            if (CatcherHelper.statusBarNotifications[position].Notification.Actions != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal string GetWhen()
        {
            try
            {
                var lol= CatcherHelper.statusBarNotifications[position].Notification.When.ToString();
                DateTime dateTime = new DateTime(Convert.ToInt64(lol));
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
    }
}