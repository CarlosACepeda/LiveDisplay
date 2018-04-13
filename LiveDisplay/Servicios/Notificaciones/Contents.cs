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
using LiveDisplay.Factories;

namespace LiveDisplay.Servicios.Notificaciones
{
    class Contents
    {
        //Item1 es 'Paquete'
        //Item2 es 'Titulo'
        //Item3 es 'Texto'
        public static Tuple<string, string, string> RetrieveNotificationContents(int whichNotification)
        {
            return Tuple.Create
            (
                Catcher.listaNotificaciones[whichNotification].PackageName,
                Catcher.listaNotificaciones[whichNotification].Notification.Extras.GetString("android.title"),
                Catcher.listaNotificaciones[whichNotification].Notification.Extras.GetString("android.text")
            );
        }
    }
}