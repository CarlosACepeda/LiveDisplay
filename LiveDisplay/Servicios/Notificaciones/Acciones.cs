using Android.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios.Notificaciones
{
    internal class Acciones
    {
        public static PendingIntent RetrieveNotificationAction(int whichNotification)
        {
            var accionNotificacion = CatcherHelper.statusBarNotifications[whichNotification].Notification.ContentIntent;
            return accionNotificacion;
        }

        public static List<Button> RetrieveNotificationButtonsActions(int whichNotification, string paquete)
        {
            List<Button> buttons = new List<Button>();
            var actions = CatcherHelper.statusBarNotifications[whichNotification].Notification.Actions;
            //Fix me
            int pixels = DpToPx(30);
            if (actions != null)
            {
                double weight = (double)1 / actions.Count;
                float weightfloat= 
                float.Parse(weight.ToString());
                
                foreach (var a in actions)
                {
                    
                    Button anActionButton = new Button(Application.Context)
                    {
                        LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, weightfloat),
                        
                        Text = a.Title.ToString(),
                       
                    };
                    anActionButton.SetMaxLines(1);
                    anActionButton.Click += (o, e)=> 
                    {
                        try
                        {

                            a.GetRemoteInputs();
                            a.ActionIntent.Send();
                            LockScreenActivity.lockScreenInstance.OnNotificationUpdated();
                        }
                        catch(Exception ex)
                        {
                            Log.Error("Action button ex:", ex.ToString());
                        }
                    };
                    
                    anActionButton.Gravity = GravityFlags.CenterVertical;
                    TypedValue typedValue = new TypedValue();
                    Application.Context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, typedValue, true);
                    anActionButton.SetBackgroundResource(typedValue.ResourceId);
                    anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(IconFactory.ReturnActionIconDrawable(a.Icon, paquete), null, null, null);
                    buttons.Add(anActionButton);
                    
                }
                return buttons;
            }
            return null;
        }


        //>Nougat: Textbox directly in notification
        public void RetrieveSendButtonAction()
        {

        }

        private static int DpToPx(int dp)
        {
            float density = Application.Context.Resources.DisplayMetrics.Density;
            return Convert.ToInt32(Math.Round(dp * density));
        }
    }
}