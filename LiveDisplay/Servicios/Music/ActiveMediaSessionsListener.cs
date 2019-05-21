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

        public void OnActiveSessionsChanged(IList<MediaController> controllers)
        {
           //Pick the best mediacontroller.
           if(controllers.Count>0)
           foreach(var mediacontroller in controllers)
           {
                if(mediacontroller?.GetTransportControls()!= null)//Ensure that this session has transport controls we can control
                {
                    mediaController= mediacontroller;
                    try
                    {
                        musicController = MusicController.GetInstance();
                        mediaController.RegisterCallback(musicController);
                        //Retrieve the controls to control the media, duh.
                        musicController.TransportControls = mediaController.GetTransportControls();
                        musicController.MediaMetadata = mediaController.Metadata;
                        musicController.PlaybackState = mediaController.PlaybackState;
                    }
                    catch
                    {
                        mediaController?.UnregisterCallback(musicController);
                        musicController.Dispose();
                    }
                    break;
                }
           }                           
        }
    }
}
