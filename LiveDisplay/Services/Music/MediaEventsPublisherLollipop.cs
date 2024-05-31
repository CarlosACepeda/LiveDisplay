using Android.App;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Util;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media.MediaEventArgs;
using System;
using System.Threading;

namespace LiveDisplay.Services.Media
{
    /// <summary>
    /// This class acts as a media session, receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks
    /// For Lollipop and beyond.
    /// </summary>
    internal class MediaEventsPublisherLollipop : MediaController.Callback, IMediaEventsPublisher
    {
        PlaybackState _playbackState;
        MediaController.TransportControls _transportControls;
        MediaMetadata _mediaMetadata;
        PendingIntent _activityIntent;
        MediaController _mediaController;
        MediaSession.Token _token;
        string _appname;
        MediaControlsBase _controls;
        static MediaEventsPublisherLollipop instance;
        const int MillisToRepeat = 2500;
        const int OneSecondInMillis = 1000;
        long currentProgress = 0;
        long totalProgress = 0;
        bool resendingMediaMetadata = false;


        System.Timers.Timer progressTimer= new System.Timers.Timer();

        private int optionSet;

        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;

        public static event EventHandler<MediaProgressChangedEventArgs> MediaProgressChanged;

        public static event EventHandler<int> MediaRepeatOptionChanged;

        public static void Initialize(MediaController controller)
        {
            if (instance == null)
                instance = new MediaEventsPublisherLollipop(controller);
            else if (controller.SessionToken.ToString() == instance._token.ToString())
            {
                //it's initialized, so lets send the media  metadata instead.
                instance.resendingMediaMetadata = true;
                instance.OnMetadataChanged(controller.Metadata);
                instance.OnPlaybackStateChanged(controller.PlaybackState);
                Log.Warn("LiveDisplay", "RESENDING DATA, ALREADY INIT");

            }
            else
            {
                //a new MediaSession requests to be initialized,
                instance.Finish(instance._token); //Finishing old
                instance = new MediaEventsPublisherLollipop(controller);
                Console.WriteLine("SUCCESS new MediaSession request, Switching...");
                Log.Warn("LiveDisplay", "SWITCHING");

            }
        }
        public static void InitializeFromToken(MediaSession.Token token)
        {
            if (token == null) throw new ArgumentNullException("Token can't be null!!");
            Initialize(new MediaController(Application.Context, token));
        }
        private MediaEventsPublisherLollipop(MediaController controller)
        {
            _mediaController = controller;
            _mediaController.RegisterCallback(this);
            _token = _mediaController.SessionToken;
            LoadMediaControllerData(_mediaController);
            _controls = MediaControlsLollipop.GetInstance();
            _controls.MediaEvent += MediaEvent;
            progressTimer.Interval = OneSecondInMillis;
            progressTimer.Elapsed += OnProgressTimerElapsed;
            Log.Warn("LiveDisplay", "CTOR SUCCESS");
        }
        public static MediaEventsPublisherLollipop GetInstance()
        {
            if (instance == null) throw new InvalidOperationException("Call Initialize or InitializeFromToken First");
            return instance;
        }
        private void LoadMediaControllerData(MediaController controller)
        {
            if (controller != null)
            {
                _transportControls = controller.GetTransportControls();

                try
                {
                    _activityIntent = controller.SessionActivity ?? PendingIntent.GetActivity(Application.Context, (int)Result.Ok, PackageUtils.GetAppIntent(controller.PackageName), PendingIntentFlags.Immutable | PendingIntentFlags.OneShot);
                }
                catch( Exception ex)
                {
                    Console.WriteLine(ex);
                }
                _appname = PackageUtils.GetTheAppName(controller.PackageName);
                //Invoke MediaMetadata, MediaPlayback, RepeatOption changed events, so all listeners will get notified of
                //the new Loaded mediacontroller.
                OnMetadataChanged(controller.Metadata);
                OnPlaybackStateChanged(controller.PlaybackState);
                OnMediaRepeatOptionChanged(optionSet);
                TrackProgress(_playbackState.Position);
            }
            else
            {
                throw new InvalidOperationException("How's this even possible?");
            }
        }

        public bool IsMediaSessionUsingToken(MediaSession.Token tokenToCheck)
        {
            return _mediaController.SessionToken.ToString() == tokenToCheck?.ToString();
        }
        public static bool IsInitialized()
        {
            return instance != null;
        }
        public bool IsActive()
        {
            if(Build.VERSION.SdkInt>= BuildVersionCodes.S)
            {
                return _playbackState.IsActive;
            }
            else
            {
                return _playbackState.State == PlaybackStateCode.Buffering ||
                    _playbackState.State == PlaybackStateCode.Connecting ||
                    _playbackState.State == PlaybackStateCode.FastForwarding ||
                    _playbackState.State == PlaybackStateCode.Playing ||
                    _playbackState.State == PlaybackStateCode.Rewinding ||
                    _playbackState.State == PlaybackStateCode.SkippingToNext ||
                    _playbackState.State == PlaybackStateCode.SkippingToPrevious ||
                    _playbackState.State == PlaybackStateCode.SkippingToQueueItem;

            }
        }
        private void MediaEvent(object sender, MediaActionEventArgs e)
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
                    if(_mediaMetadata.GetLong(MediaMetadata.MetadataKeyDuration)> 0) //in Live streams this value is 0. so we use it to prevent unwanted seek.
                        _transportControls?.SeekTo(e.Time);
                    break;

                case MediaActionFlags.FastFoward:
                    if (_mediaMetadata.GetLong(MediaMetadata.MetadataKeyDuration) > 0) 
                        _transportControls?.FastForward();
                    break;

                case MediaActionFlags.Rewind:
                    if (_mediaMetadata.GetLong(MediaMetadata.MetadataKeyDuration) > 0) 
                        _transportControls?.Rewind();
                    break;

                case MediaActionFlags.Stop:
                    _transportControls?.Stop();
                    break;

                case MediaActionFlags.RetrieveMediaInformation:

                    break;
                case MediaActionFlags.CycleRepeatOption:
                    CycleRepeatOption();
                    OnMediaRepeatOptionChanged(optionSet);
                    break;
                case MediaActionFlags.OpenRelatedActivity:
                    OpenRelatedActivity();
                    break;
                default:
                    break;
            }
        }

        private void OpenRelatedActivity()
        {
            try
            {
                _activityIntent?.Send();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Couldn't launch related activity: {ex}");
            }
        }

        public override void OnPlaybackStateChanged(PlaybackState state)
        {
            _playbackState = state;
            TrackProgress(state.Position);

            OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
            {
                PlaybackState = state.State,
                CurrentTime = state.Position,
                RepeatOptionSet= optionSet
            });
            base.OnPlaybackStateChanged(state);

        }

        public override void OnMetadataChanged(MediaMetadata metadata)
        {
            //This method gets called many times,
            //I shoud investigate what is in the Metadata changing so much when I just do something as simple as restart a song.
            //for now, let's compare the information I'm interested in, and if it differs somehow then actually invoke the OnMediaMetadataChanged.

            bool isAnythingDifferent;
            var title = _mediaMetadata?.GetString(MediaMetadata.MetadataKeyTitle);
            var artist = _mediaMetadata?.GetString(MediaMetadata.MetadataKeyArtist);
            var album = _mediaMetadata?.GetString(MediaMetadata.MetadataKeyAlbum);
            var duration = _mediaMetadata?.GetLong(MediaMetadata.MetadataKeyDuration);
            var albumart = _mediaMetadata?.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);


            var incomingTitle = metadata.GetString(MediaMetadata.MetadataKeyTitle);
            var incomingArtist = metadata.GetString(MediaMetadata.MetadataKeyArtist);
            var incomingAlbum = metadata.GetString(MediaMetadata.MetadataKeyAlbum);
            var incomingDuration = metadata.GetLong(MediaMetadata.MetadataKeyDuration); 
            var incomingAlbumArt= metadata?.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);

            var titleDifferent = title != incomingTitle;
            var artistDifferent = artist != incomingArtist;
            var albumDifferent = album != incomingAlbum;
            var durationDifferent = duration != incomingDuration;
            var albumartDifferent = albumart!=null && !albumart.SameAs(incomingAlbumArt);


            isAnythingDifferent = albumartDifferent || titleDifferent || artistDifferent || albumDifferent || durationDifferent;
            
            if (isAnythingDifferent || resendingMediaMetadata)
            {
                _mediaMetadata = metadata;
                totalProgress = incomingDuration;

                OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                {
                    ActivityIntent = _activityIntent,
                    MediaMetadata = _mediaMetadata,
                    AppName = _appname
                });

                if(resendingMediaMetadata) resendingMediaMetadata = false;
            }

            base.OnMetadataChanged(_mediaMetadata);
        }

        public void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e)
        { 
            ThreadPool.QueueUserWorkItem(m =>
            {
                MediaPlaybackChanged?.Invoke(this, e);
            });
        }

        public void OnMediaMetadataChanged(EventArgs e)

        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                MediaMetadataChanged?.Invoke(this, (MediaMetadataChangedEventArgs)e);
            });
        }

        public bool Finish(MediaSession.Token mediaSessionTokenToFinish)
        {
            if (mediaSessionTokenToFinish == null) return false; 
            if (_token?.ToString() == mediaSessionTokenToFinish.ToString())
            {
                try
                {
                    _mediaController.UnregisterCallback(instance);
                    _controls.MediaEvent -= MediaEvent;
                    progressTimer.Elapsed -= OnProgressTimerElapsed;
                }
                catch (Exception ex)
                {
                    Log.Warn("LiveDisplay", $"Failed Finishing!! {ex.Message}");
                }
                instance = null;
                return true;
            }
            return false;
        }
        public override void OnSessionDestroyed()
        {
            Console.WriteLine("SessionDestroyed CALLED");
            //Self destroy instance in this case.
            Finish(_token);
            base.OnSessionDestroyed();
        }

        void TrackProgress(long currentPos)
        {
            currentProgress = currentPos;
            switch(_playbackState.State)
            {
                case PlaybackStateCode.Playing:
                    progressTimer.Start();
                    break;
                default:
                    progressTimer.Stop();
                    break;
            }
        }
        public void OnProgressTimerElapsed(object sender, EventArgs e)
        {
            currentProgress += 1000;
            OnMediaProgressChanged(new MediaProgressChangedEventArgs
            {
                CurrentProgress = currentProgress,
                TotalProgress = _mediaMetadata.GetLong(MediaMetadata.MetadataKeyDuration)
            });
            if (optionSet != IMediaEventsPublisher.DontRepeat)
            {
                if (totalProgress - currentProgress <= MillisToRepeat)
                {
                    var mediaControls = MediaControlsLollipop.GetInstance();
                    mediaControls?.Pause();
                    mediaControls?.SeekTo(0);
                    mediaControls?.Play();

                    if (optionSet == IMediaEventsPublisher.RepeatOnce)
                    {
                        optionSet = IMediaEventsPublisher.DontRepeat;
                    }
                    OnMediaRepeatOptionChanged(optionSet);
                    Console.WriteLine("REPEATING!!!");
                }
            }
        }

        public void OnMediaProgressChanged(MediaProgressChangedEventArgs e)
        {
            MediaProgressChanged?.Invoke(null, e);
        }
        public void CycleRepeatOption()
        {
            optionSet++;
            if(optionSet > IMediaEventsPublisher.RepeatForever)
            {
                optionSet = IMediaEventsPublisher.DontRepeat;
            }
        }
        public void OnMediaRepeatOptionChanged(int newOption)
        {
            MediaRepeatOptionChanged?.Invoke(null, newOption);
        }
    }
}