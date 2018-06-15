using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Notificaciones
{
    /// <summary>
    /// This class acts a a Listener for MediaSessions being created
    /// So, when a Session is created, I catch that Session and Use it to Control Media of tha session
    /// through the Jukebox class.
    /// </summary>
    class ActiveMediaSessionsListener : Java.Lang.Object, MediaSessionManager.IOnActiveSessionsChangedListener
    {
        public void OnActiveSessionsChanged(IList<Android.Media.Session.MediaController> controllers)
        {
            //TODO: Move code from Catcher to here, Register the Callback here.
            //And jukebox too.
            Console.WriteLine("ActiveSessionListener called.");
            if (controllers.Count > 0)
            {
                Console.WriteLine("There is 1 or more active Sessions");
            }
            else if(controllers.Count==0)
            {
                Console.WriteLine("No hay sesiones Activas");
            }
        }
    }
}