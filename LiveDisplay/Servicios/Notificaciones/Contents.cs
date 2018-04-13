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
    class Contents
    {
        public static object RetrieveNotificationContents(int whichNotification)
        {
            return new
            {
                paquete = Catcher.listaNotificaciones[whichNotification].PackageName,
                titulo = Catcher.listaNotificaciones[whichNotification].Notification.Extras.GetString("android.title"),
                text = Catcher.listaNotificaciones[whichNotification].Notification.Extras.GetString("android.text"),
            };
        }
    }
}