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
           // TODO: Very Unstable 
            return Tuple.Create
            (
                //Showwhen?
                //Appname?
                //Time?
                CatcherHelper.statusBarNotifications[whichNotification].PackageName,
                CatcherHelper.statusBarNotifications[whichNotification].Notification.Extras.Get("android.title").ToString(),
                CatcherHelper.statusBarNotifications[whichNotification].Notification.Extras.Get("android.text").ToString()
                
            );

        }
    }
}