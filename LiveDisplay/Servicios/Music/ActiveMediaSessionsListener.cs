using Android.Media.Session;
using Android.Util;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class acts a a Listener for MediaSessions being created
    /// So, when a Session is created, I catch that Session and Use it to Control Media of tha session
    /// through the Jukebox class.
    /// </summary>
    internal class ActiveMediaSessionsListener : Java.Lang.Object, MediaSessionManager.IOnActiveSessionsChangedListener
    {
        private MediaController mediaController;

        private MusicController musicController;

        //Al parecer hay varios controladores de Multimedia y toca recuperarlos.
        public void OnActiveSessionsChanged(IList<MediaController> controllers)
        {
            if (controllers.Count > 0)
            {
                mediaController = controllers[0];
                mediaController.RegisterCallback(musicController = MusicController.GetInstance());

                //Retrieve the controls to control the media, duh.
                musicController.TransportControls = mediaController.GetTransportControls();
                musicController.MediaMetadata = mediaController.Metadata;
                musicController.PlaybackState = mediaController.PlaybackState;
            }
            else if (mediaController != null && controllers.Count == 0)
            {
                Log.Info("LiveDisplay", "mediacontroller null or no controllers.");
                //This is probably never to happen
                mediaController.UnregisterCallback(musicController);
                musicController.Dispose();
            }
        }

    }
}