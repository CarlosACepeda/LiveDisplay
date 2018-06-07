using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using LiveDisplay.Factories;

namespace LiveDisplay.Servicios.Notificaciones
{
    class OpenNotification: IDisposable
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
                return CatcherHelper.statusBarNotifications[position].Notification.Extras.Get("android.title").ToString();
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
                return CatcherHelper.statusBarNotifications[position].Notification.Extras.Get("android.text").ToString();
            }
            catch
            {
                return "";
            }
        }
        public static void ClickNotification(int position)
        {
            var pendingIntent = CatcherHelper.statusBarNotifications[position].Notification.ContentIntent;
            try
            {
                pendingIntent.Send();
            }
            catch
            {
                System.Console.WriteLine("Click Notification failed, fail in pending intent");
            }
            pendingIntent.Dispose();
        }
        public static List<Button> RetrieveActionButtons(int position)
        {
            List<Button> buttons = new List<Button>();
            var actions = CatcherHelper.statusBarNotifications[position].Notification.Actions;
            if (actions != null)
            {
                double weight = (double)1 / actions.Count;
                float weightfloat =
                float.Parse(weight.ToString());
                string paquete = CatcherHelper.statusBarNotifications[position].PackageName;
                foreach (var a in actions)
                {

                    Button anActionButton = new Button(Application.Context)
                    {
                        LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, weightfloat),

                        Text = a.Title.ToString(),

                    };
                    anActionButton.SetMaxLines(1);
                    anActionButton.Click += (o, e) =>
                    {
                        try
                        {

                            a.GetRemoteInputs();
                            a.ActionIntent.Send();
                            LockScreenActivity.lockScreenInstance.OnNotificationUpdated();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Action button ex:", ex.ToString());
                        }
                    };

                    anActionButton.Gravity = GravityFlags.CenterVertical;
                    TypedValue typedValue = new TypedValue();
                    Application.Context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, typedValue, true);
                    anActionButton.SetBackgroundResource(typedValue.ResourceId);
                    anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(IconFactory.ReturnActionIconDrawable(a.Icon,paquete), null, null, null);
                    buttons.Add(anActionButton);

                }
                return buttons;
            }
            return null;
        }
        public static bool NotificationHasActionButtons(int position)
        {
            if (CatcherHelper.statusBarNotifications[position].Notification.Actions.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Dispose()
        {
            
        }
        
    }
}