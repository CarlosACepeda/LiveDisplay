using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Notificaciones
{
    class Acciones
    {
        public static PendingIntent RetrieveNotificationAction(int whichNotification)
        {
            using (var accionNotificacion= Catcher.listaNotificaciones[whichNotification].Notification.ContentIntent)
            {
                return accionNotificacion;
            }                
        }
        public static List<Button> RetrieveNotificationButtonsActions(int whichNotification)
        {
            List<Button> buttons = new List<Button>();
            var actions = Catcher.listaNotificaciones[whichNotification].Notification.Actions;

            foreach (var a in actions)
            {
                Button anActionButton = new Button(Application.Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(100, 100),                   
                };
                buttons.Add(anActionButton);
            }
            return buttons= actions!=null? buttons: null;
        }
        //>Nougat: Textbox directly in notification
        public void RetrieveSendButtonAction()
        { }
    }
}