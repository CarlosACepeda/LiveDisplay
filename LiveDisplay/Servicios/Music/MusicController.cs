using Android.App;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Media.Session;
using Android.Util;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using LiveDisplay.Servicios.Notificaciones;
using System;
using System.Net;
using System.Runtime.InteropServices;
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
        static PlaybackState _playbackState;
        static MediaController.TransportControls _transportControls;
        static MediaMetadata _mediaMetadata;
        private static MusicController instance;
        private static PendingIntent _activityIntent;
        private static MediaController _currentMediaController;
        private static MediaSession.Token _currentToken;
        private static bool _playbackstarted;
        private static string _appname;
        private static string _openNotificationId;

        #region events

        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;

        public static event EventHandler MusicPlaying;

        public static event EventHandler MusicPaused;

        #endregion events

        #endregion Class members

        /// <summary>
        /// Pass a MediaSession.Token to create one MediaController.
        /// 
        /// </summary>
        /// <param name="mediaController"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool StartPlayback(MediaSession.Token token, string owningNotificationId)
        {
            if (instance == null)
            {
                if (token != null)
                {
                    _currentMediaController = new MediaController(Application.Context, token);
                    _currentToken = token;
                }
                else
                {
                    throw new ArgumentException("Token can't be null!");
                }
                GetCurrentInstance(_currentMediaController, owningNotificationId);
                return StartMediaPlayback();
            }
            else 
            {
                if (IsPlaybackStarted(token) == false)
                {
                    StopPlayback(_currentToken); //the incoming token is different so I will stop the previous media callback before
                                                                          //creating a new one.

                    _currentMediaController = new MediaController(Application.Context, token);
                    _currentToken = token;
                    LoadMediaControllerData(_currentMediaController);
                    return StartMediaPlayback();
                }
                else 
                {
                    //The mediaplayback is already started for this session in particular.

                    if (_openNotificationId != owningNotificationId) //It means that the session is the same, but the notification owning this session changed.
                    {
                        _openNotificationId = owningNotificationId; //Update the notification (taking its id) that way we can identify the notification that owns this media session.
                    }
                    return false;
                }
            }
        }
        private static MusicController GetCurrentInstance(MediaController controller, string owningNotificationId)
        {
            if (instance == null)
                instance = new MusicController(controller, owningNotificationId);
            return instance;
        }
        private MusicController(MediaController controller, string notificationId)
        {
            LoadMediaControllerData(controller);
            _openNotificationId = notificationId;
            Jukebox.MediaEvent += Jukebox_MediaEvent;
        }
        private static void LoadMediaControllerData(MediaController controller)
        {
            if (controller != null)
            {
                _transportControls = controller.GetTransportControls();
                _mediaMetadata = controller.Metadata;
                _playbackState = controller.PlaybackState;
                _activityIntent = controller.SessionActivity;
                _appname = PackageUtils.GetTheAppName(controller.PackageName);
                //Invoke MediaMetadata and MediaPlayback changed events, so all listeners will get notified of
                //the new Loaded mediacontroller.
                instance?.OnMetadataChanged(controller.Metadata);
                instance?.OnPlaybackStateChanged(controller.PlaybackState);
            }
        }

        private void Jukebox_MediaEvent(object sender, MediaActionEventArgs e)
        {
            switch (e.MediaActionFlags)
            {
                case MediaActionFlags.Play:
                    _transportControls?.Play();
                    break;

                case MediaActionFlags.Pause:
                    _transportControls?.Pause();
                    break;

                case MediaActionFlags.SkipToNext:
                    _transportControls?.SkipToNext();
                    break;

                case MediaActionFlags.SkipToPrevious:
                    _transportControls?.SkipToPrevious();
                    break;

                case MediaActionFlags.SeekTo:
                    _transportControls?.SeekTo(e.Time);
                    break;

                case MediaActionFlags.FastFoward:
                    _transportControls?.FastForward();
                    break;

                case MediaActionFlags.Rewind:
                    _transportControls?.Rewind();
                    break;

                case MediaActionFlags.Stop:
                    _transportControls?.Stop();
                    break;

                case MediaActionFlags.RetrieveMediaInformation:
                    //Send media information.
                    OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                    {
                        MediaMetadata = _mediaMetadata,
                        ActivityIntent = _activityIntent,
                        AppName= _appname,
                        OpenNotificationId = _openNotificationId
                    });
                    //Send Playbackstate of the media.
                    OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
                    {
                        PlaybackState = _playbackState.State,
                        CurrentTime = _playbackState.Position
                    });

                    break;

                default:
                    break;
            }
        }

        internal static bool MediaSessionAssociatedWThisNotification(string openNotificationId)
        {
            return _openNotificationId == openNotificationId; //Useful to check which notification (In case a notification causes this MusicController to be ative) owns this MusicController
        }

        public override void OnPlaybackStateChanged(PlaybackState state)
        {
            _playbackState = state;
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
            _mediaMetadata = metadata;

            OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
            {
                ActivityIntent = _activityIntent,
                MediaMetadata = _mediaMetadata,
                AppName= _appname,
                OpenNotificationId = _openNotificationId
            });
            //Datos de la Media que se está reproduciendo.

            base.OnMetadataChanged(_mediaMetadata);
            //Memory is growing until making a GC.
            GC.Collect();
        }

        #region Raising events.

        protected virtual void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                switch (e.PlaybackState)
                {
                    case PlaybackStateCode.Playing:
                        MusicPlaying?.Invoke(this, EventArgs.Empty);
                        break;

                    case PlaybackStateCode.Paused:
                        MusicPaused?.Invoke(this, EventArgs.Empty);
                        break;
                }
                MediaPlaybackChanged?.Invoke(this, e);
            });
        }

        protected virtual void OnMediaMetadataChanged(MediaMetadataChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                MediaMetadataChanged?.Invoke(this, e);
            });
        }

        #endregion Raising events.

        protected override void Dispose(bool disposing)
        {
            //release resources.

            base.Dispose(disposing);
        }

        private static bool StartMediaPlayback()
        {
            if (_currentMediaController == null)
            {
                Log.Warn("LiveDisplay", "current Media controller is null!");
            }
            try
            {
                _currentMediaController.RegisterCallback(instance);
                _playbackstarted = true;
                return true;
            }
            catch (Exception)
            {
                Log.Warn("LiveDisplay", "Failed MusicController#StartMediaCallback");
                _playbackstarted = false;
                return false;
            }
        }
        //if you don't pass any argument it'll effectively do nothing.
        public static bool StopPlayback(MediaSession.Token token= null)
        {
            if (_currentToken == token) //Making sure we are stopping the same one we started.
            {
                try
                {
                    _currentMediaController.UnregisterCallback(instance);
                    _playbackstarted = false;
                    _openNotificationId = null;
                    return true;
                }
                catch (Exception)
                {
                    Log.Warn("LiveDisplay", "Failed MusicController#StopMediaCallback");
                    _playbackstarted = false;
                    _openNotificationId = null;
                    return false;
                }
            }
            return false;
        }
        private static bool IsPlaybackStarted(MediaSession.Token token)
        {
            if (_currentToken.Equals(token) && _playbackstarted == true)
            {
                return true;
            }
            return false;
        }
        public override void OnSessionDestroyed()
        {
            StopPlayback(_currentToken); //Just in case... to avoid memory leaks.
            Jukebox.MediaEvent -= Jukebox_MediaEvent;
            _playbackState?.Dispose();
            _transportControls?.Dispose();
            _mediaMetadata?.Dispose();
            _currentMediaController?.Dispose();
            instance = null;
            Log.Info("LiveDisplay", "MusicController dispose method");
            base.OnSessionDestroyed();
        }
    }
}