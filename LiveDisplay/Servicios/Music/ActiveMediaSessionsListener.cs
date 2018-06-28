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
using static Android.Media.Session.MediaController;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class acts a a Listener for MediaSessions being created
    /// So, when a Session is created, I catch that Session and Use it to Control Media of tha session
    /// through the Jukebox class.
    /// </summary>
    class ActiveMediaSessionsListener : Java.Lang.Object, MediaSessionManager.IOnActiveSessionsChangedListener
    {
        public static bool isASessionActive = false;
        private Android.Media.Session.MediaController mediaController;
        public static event EventHandler MediaSessionStarted;
        public static event EventHandler MediaSessionStopped;

        //Al parecer hay varios controladores de Multimedia y toca recuperarlos.
        public void OnActiveSessionsChanged(IList<Android.Media.Session.MediaController> controllers)
        {
            if (controllers.Count > 0)
            {
                Console.WriteLine("There is 1 or more active Sessions");
                mediaController = controllers[0];
                mediaController.RegisterCallback(MusicController.MusicControllerInstance());
                //To control current media playing
                GetMusicControls(mediaController.GetTransportControls());
                isASessionActive = true;

                Console.WriteLine("RemoteController registered Lollipop");
            }
            else if(controllers.Count==0)
            {
                isASessionActive = false;
                Console.WriteLine("No hay sesiones Activas");
            }
        }

        protected virtual void OnMediaSessionStarted()
        {
            MediaSessionStarted?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnMediaSessionStopped()
        {
            MediaSessionStopped?.Invoke(this, EventArgs.Empty);
        }
        private void GetMusicControls(TransportControls mediaTransportControls)
        {
            Jukebox jukebox = Jukebox.JukeboxInstance(mediaTransportControls);
            OnMediaSessionStarted();
        }
    }
}