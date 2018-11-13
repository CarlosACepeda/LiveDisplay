using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Util;
using LiveDisplay.Factories;
using LiveDisplay.Servicios.Music.MediaEventArgs;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class acts as a media session, receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks 
    /// For Lollipop and beyond.
    /// </summary>
    internal class MusicController : MediaController.Callback, IDisposable
    {
        
        
        #region Class members 
        public PlaybackState PlaybackState {get;set;}
        public MediaController.TransportControls TransportControls {get; set;}
        public MediaMetadata MediaMetadata {get; set;}
        private static MusicController instance;

        #region events
        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;
        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;
        #endregion
        #endregion

        internal static MusicController GetInstance()
        {
            if (instance == null)
            {
                instance = new MusicController();
            }
            return instance;
        }
        private MusicController()
        {
            Jukebox.MediaEvent += Jukebox_MediaEvent; //Subscribe to this event only once because this class is a Singleton.
        }
        private void Jukebox_MediaEvent(object sender, MediaActionEventArgs e)
        {
            switch (e.MediaActionFlags)
            {
                case Misc.MediaActionFlags.Play:
                    TransportControls.Play();
                    break;
                case Misc.MediaActionFlags.Pause:
                    TransportControls.Pause();
                    break;
                case Misc.MediaActionFlags.SkipToNext:
                    TransportControls.SkipToNext();
                    break;
                case Misc.MediaActionFlags.SkipToPrevious:
                    TransportControls.SkipToPrevious();
                    break;
                case Misc.MediaActionFlags.SeekTo:
                    TransportControls.SeekTo(e.Time);
                    break;
                case Misc.MediaActionFlags.FastFoward:
                    TransportControls.FastForward();
                    break;
                case Misc.MediaActionFlags.Rewind:
                    TransportControls.Rewind();
                    break;
                case Misc.MediaActionFlags.Stop:
                    TransportControls.Stop();
                    break;
                case Misc.MediaActionFlags.RetrieveMediaInformation:
                    //Send media information.
                    OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                    {
                        MediaMetadata = MediaMetadata
                    });
                    //Send Playbackstate of the media.
                    OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
                    {
                        PlaybackState= PlaybackState.State,
                        CurrentTime= PlaybackState.Position
                    });
                    
                    break;
                default:
                    break;
            }
        }
        public override void OnPlaybackStateChanged(PlaybackState state)
        {
                PlaybackState= state;
            //Estado del playback:
            //Pausado, Comenzado, Avanzando, Retrocediendo, etc.    
                OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
                {
                PlaybackState = state.State,
                CurrentTime= (int)(state.Position/1000)
                });
            base.OnPlaybackStateChanged(state);
        }
        public override void OnMetadataChanged(MediaMetadata metadata)
        {
            try
            {
                MediaMetadata = metadata;

                OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                {
                    MediaMetadata= metadata

                });
            }
            catch
            {
                Log.Info("LiveDisplay", "Failed getting metadata MusicController.");
                //Don't do anything.
            }
            

            //Datos de la Media que se está reproduciendo.            
            
            base.OnMetadataChanged(metadata);
            //Memory is growing until making a GC.
            GC.Collect();
        }
        #region Raising events.
        protected virtual void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e)
        {
            MediaPlaybackChanged?.Invoke(this, e);
        }
        protected virtual void OnMediaMetadataChanged(MediaMetadataChangedEventArgs e)
        {
            MediaMetadataChanged?.Invoke(this, e);
        }
        #endregion
    }
}
