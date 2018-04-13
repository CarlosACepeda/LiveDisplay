using Android.App;
using Android.Views;
using Android.Widget;
using LiveDisplay.Factories;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Servicios.Notificaciones
{
    internal class Acciones
    {
        public static PendingIntent RetrieveNotificationAction(int whichNotification)
        {
            var accionNotificacion = Catcher.listaNotificaciones[whichNotification].Notification.ContentIntent;
            return accionNotificacion;
        }

        public static List<Button> RetrieveNotificationButtonsActions(int whichNotification, string paquete)
        {
            List<Button> buttons = new List<Button>();
            var actions = Catcher.listaNotificaciones[whichNotification].Notification.Actions;
            int pixels = DpToPx(30);
            if (actions != null)
            {
                foreach (var a in actions)
                {
                    Button anActionButton = new Button(Application.Context)
                    {
                        LayoutParameters = new ViewGroup.LayoutParams(pixels, pixels),
                        Background = IconFactory.ReturnActionIconDrawable(a.Icon, paquete),
                        Text = a.Title.ToString()
                    };
                    anActionButton.Click += (o, e) => a.ActionIntent.Send();
                    buttons.Add(anActionButton);
                }
                return buttons;
            }
            return null;
        }

        //>Nougat: Textbox directly in notification
        public void RetrieveSendButtonAction()
        { }
        private static int DpToPx(int dp)
        {
            float density = Application.Context.Resources.DisplayMetrics.Density;
            return Convert.ToInt32(Math.Round(dp * density));
        }


    }
}