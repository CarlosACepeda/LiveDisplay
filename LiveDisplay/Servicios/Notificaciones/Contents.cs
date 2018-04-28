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
                Catcher.listaNotificaciones[whichNotification].PackageName,
                Catcher.listaNotificaciones[whichNotification].Notification.Extras.GetString("android.title"),
                Catcher.listaNotificaciones[whichNotification].Notification.Extras.GetString("android.text")
            );
        }
    }
}