using Android.App;
using Android.Media;
using Android.Media.Session;
using Android.Util;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using System;
using System.Threading;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class acts as a media session, receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks
    /// For Lollipop and beyond.
    /// </summary>
    internal class MusicController : MediaController.Callback
    {
        #region Class members

        public static PlaybackStateCode MusicStatus { get; private set; }
        public PlaybackState PlaybackState { get; set; }
        public MediaController.TransportControls TransportControls { get; set; }
        public MediaMetadata MediaMetadata { get; set; }
        private static MusicController instance;
        public static PendingIntent ActivityIntent { get; set; }

        #region events

        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;

        public static event EventHandler MusicPlaying;
        public static event EventHandler MusicPaused;

        #endregion events

        #endregion Class members

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
                case MediaActionFlags.Play:
                    TransportControls.Play();
                    break;

                case MediaActionFlags.Pause:
                    TransportControls.Pause();
                    break;

                case MediaActionFlags.SkipToNext:
                    TransportControls.SkipToNext();
                    break;

                case MediaActionFlags.SkipToPrevious:
                    TransportControls.SkipToPrevious();
                    break;

                case MediaActionFlags.SeekTo:
                    TransportControls.SeekTo(e.Time);
                    break;

                case MediaActionFlags.FastFoward:
                    TransportControls.FastForward();
                    break;

                case MediaActionFlags.Rewind:
                    TransportControls.Rewind();
                    break;

                case MediaActionFlags.Stop:
                    TransportControls.Stop();
                    break;

                case MediaActionFlags.RetrieveMediaInformation:
                    //Send media information.
                    OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                    {
                        MediaMetadata = MediaMetadata,
                        ActivityIntent= ActivityIntent
                    });
                    //Send Playbackstate of the media.
                    OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
                    {
                        PlaybackState = PlaybackState.State,
                        CurrentTime = PlaybackState.Position
                    });

                    break;

                default:
                    break;
            }
        }

        public override void OnPlaybackStateChanged(PlaybackState state)
        {
            PlaybackState = state;
            MusicStatus = state.State;
            OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
            {
                PlaybackState = state.State,
                CurrentTime = state.Position
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
                    MediaMetadata = metadata
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
            ThreadPool.QueueUserWorkItem(m =>
            {
                MediaPlaybackChanged?.Invoke(this, e);
            });
        }

        protected virtual void OnMediaMetadataChanged(MediaMetadataChangedEventArgs e)
        {
            MediaMetadataChanged?.Invoke(this, e);
        }

        #endregion Raising events.

        protected override void Dispose(bool disposing)
        {
            //release resources.

            base.Dispose(disposing);
        }

        public override void OnSessionDestroyed()
        {
            
            Jukebox.MediaEvent -= Jukebox_MediaEvent;
            PlaybackState?.Dispose();
            TransportControls?.Dispose();
            MediaMetadata?.Dispose();
            instance = null;
            Log.Info("LiveDisplay", "MusicController dispose method");
            base.OnSessionDestroyed();
        }
    }
}