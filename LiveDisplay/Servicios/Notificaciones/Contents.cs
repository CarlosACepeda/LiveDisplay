using Android.Util;
using System;

namespace LiveDisplay.Servicios.Notificaciones
{
    internal class Contents
    {
        //Item1 es 'Paquete'
        //Item2 es 'Titulo'
        //Item3 es 'Texto'
        public static Tuple<string, string, string> RetrieveNotificationContents(int whichNotification)
        {
            
            return Tuple.Create
            (
                //Showwhen?
                //Appname?
                //Time?
                Catcher.listaNotificaciones[whichNotification].PackageName,
                Catcher.listaNotificaciones[whichNotification].Notification.Extras.Get("android.title").ToString(),
                Catcher.listaNotificaciones[whichNotification].Notification.Extras.Get("android.text").ToString()
                
            );

        }
    }
}