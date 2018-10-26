using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Util;
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
        public static bool IsASessionActive {get; set; }
        private Android.Media.Session.MediaController mediaController;
        public static event EventHandler MediaSessionStarted;
        public static event EventHandler MediaSessionStopped;
        private MusicController musicController;
        OpenSong song = OpenSong.OpenSongInstance();
        Bitmap bitmap;

        //Al parecer hay varios controladores de Multimedia y toca recuperarlos.
        public void OnActiveSessionsChanged(IList<Android.Media.Session.MediaController> controllers)
        {
            Log.Info("LiveDisplay", "OnActiveSessions Changed");
            if (controllers.Count > 0)
            {
                Log.Info("LiveDisplay", "controllers: "+ controllers.Count);
                mediaController = controllers[0];
                mediaController.RegisterCallback(musicController = new MusicController());
                
                //Retrieve current media information if any, because it could be that an app starts a new MediaSession without media playing.
                GetCurrentMetadata();
                //To control current media playing
                GetMusicControls(mediaController.GetTransportControls());
                IsASessionActive = true;
            }
            else if(mediaController!=null && controllers.Count==0)
            {
                Log.Info("LiveDisplay", "mediacontroller null or no controllers.");
                //This is probably never to happen
                mediaController.UnregisterCallback(musicController);
                    IsASessionActive = false;
            }
        }

        private void GetCurrentMetadata()
        {
            
            int size = (int)Application.Context.Resources.GetDimension(Resource.Dimension.albumartsize);
            try
            {
                bitmap = Bitmap.CreateScaledBitmap(mediaController.Metadata.GetBitmap(MediaMetadata.MetadataKeyAlbumArt), size, size, true);
            }
            catch
            {
                //There is not bitmap
            }


            try
            {
                song.Title = mediaController.Metadata.GetText(MediaMetadata.MetadataKeyTitle);
                song.Artist = mediaController.Metadata.GetText(MediaMetadata.MetadataKeyArtist);
                song.Album = mediaController.Metadata.GetText(MediaMetadata.MetadataKeyAlbum);
                song.Duration = (int)mediaController.Metadata.GetLong(MediaMetadata.MetadataKeyDuration);
                song.PlaybackState = mediaController.PlaybackState.State;
                song.AlbumArt = bitmap;
                bitmap = null;
                Log.Info("LiveDisplay", "Metadata Retrieved");
            }
            catch
            {
                Log.Info("LiveDisplay", "Failed to get Metadata/Started a mediaSession without media playing.");
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
        
    }
}