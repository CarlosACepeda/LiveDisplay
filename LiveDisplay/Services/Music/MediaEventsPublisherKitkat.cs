using Android.App;
using Android.Graphics;
using Android.Media;
using Android.Util;
using Android.Views;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media.Enums;
using LiveDisplay.Services.Media.MediaEventArgs;
using LiveDisplay.Services.Notifications;
using System;
using System.Runtime.Remoting.Messaging;


namespace LiveDisplay.Services.Media
{
    /// <summary>
    /// This class receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks
    /// For Kitkat only.
    /// </summary>

    internal class MediaEventsPublisherKitkat : IMediaEventsPublisher, IDisposable
    {
        private static MediaEventsPublisherKitkat instance;
        private int repeatOptionSet;
        long currentProgress;
        long totalProgress;
        const int MillisToRepeat = 2500;
        const int OneSecondInMillis = 1000;
        System.Timers.Timer progressTimer = new System.Timers.Timer();
        PendingIntent _activityIntent;
        private RemoteControlFlags _remoteControlFlags;

        public RemoteControlPlayState PlaybackState { get; set; }
        public RemoteController.MetadataEditor MediaMetadata { get; set; }
        public RemoteController TransportControls { get; set; }

        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;

        public static event EventHandler<MediaProgressChangedEventArgs> MediaProgressChanged;
        public static event EventHandler<int> MediaRepeatOptionChanged;
        public static event EventHandler<ControlsAvailabilityChangedEventArgs> ControlsAvailabilityChanged;

        private MediaEventsPublisherKitkat(RemoteController remoteController)
        {
            MediaControlsBase.Instance.MediaEvent += MusicControlsKitkat_MediaEvent;
            progressTimer.Interval = OneSecondInMillis;
            progressTimer.Elapsed += OnProgressTimerElapsed;
            TransportControls = remoteController;
        }
        public static MediaEventsPublisherKitkat Initialize(RemoteController remoteController)
        {
           instance??= new MediaEventsPublisherKitkat(remoteController);
            return instance;
        }
        public static MediaEventsPublisherKitkat GetInstance()
        {
            if (instance == null) throw new InvalidOperationException("Call Initialize First");
            return instance;
        }
        public static bool IsInitialized()
        {
            return instance != null;
        }

        private void MusicControlsKitkat_MediaEvent(object sender, MediaActionEventArgs e)
        {
            RemoteControlPlayState simulatedState= RemoteControlPlayState.Error;
            switch (e.MediaActionFlags)
            {

                case MediaActionFlags.Play:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaPlay));
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.MediaPlay));
                    simulatedState = RemoteControlPlayState.Playing;
                    break;

                case MediaActionFlags.Pause:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaPause));
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.MediaPause));
                    simulatedState = RemoteControlPlayState.Paused;
                    break;

                case MediaActionFlags.SkipToNext:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaNext));
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.MediaNext));
                    simulatedState = RemoteControlPlayState.SkippingForwards;
                    break;

                case MediaActionFlags.SkipToPrevious:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaPrevious));
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.MediaPrevious));
                    simulatedState = RemoteControlPlayState.SkippingBackwards;
                    break;

                case MediaActionFlags.SeekTo:
                    TransportControls.SeekTo(e.Time);
                    //Test this case
                    break;

                case MediaActionFlags.FastFoward:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaFastForward));
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.MediaFastForward));
                    simulatedState = RemoteControlPlayState.FastForwarding;
                    break;

                case MediaActionFlags.Rewind:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaRewind));
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.MediaRewind));
                    simulatedState = RemoteControlPlayState.Rewinding;
                    break;

                case MediaActionFlags.Stop:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaStop));
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.MediaStop));
                    simulatedState = RemoteControlPlayState.Stopped;

                    break;

                case MediaActionFlags.RetrieveMediaInformation:
                    break;

                case MediaActionFlags.CycleRepeatOption:
                    CycleRepeatOption();
                    OnMediaRepeatOptionChanged(repeatOptionSet);
                    break;
                case MediaActionFlags.OpenRelatedActivity:
                    OpenRelatedActivity();
                    break;
                default:
                    break;
            }
            Console.WriteLine("Sending Mediaplayback Event manually, cuz apparently after sending the key event the media the RemoteController doesn't react");
            //Send Playbackstate of the media Manually
            OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
            {
                PlaybackStateKitkat = simulatedState,
                CurrentTime = TransportControls.EstimatedMediaPosition,
                RepeatOptionSet= repeatOptionSet
            });
        }
        private void OpenRelatedActivity()
        {
            try
            {
                _activityIntent?.Send();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't launch related activity: {ex}");
            }
        }
        public void OnTransportControlsUpdate(RemoteControlFlags remoteControlFlags)
        {
            _remoteControlFlags = remoteControlFlags;
        }
        public void OnPlaybackStateChanged(RemoteControlPlayState state)
        {
            PlaybackState = state;
            TrackProgress(TransportControls.EstimatedMediaPosition);

            Log.Info("LiveDisplay", "Music state is: " + state);
            OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
            {
                PlaybackStateKitkat = state,
                CurrentTime= TransportControls.EstimatedMediaPosition,
                RepeatOptionSet= repeatOptionSet,
            });
            OnControlsAvailabilityChanged(new ControlsAvailabilityChangedEventArgs
            {
                AvailableControls = SetAvailableControls(GetSupportedActions()),
                CustomActions = null,
                TakeCustomActionsFromNotification = true, //In kitkat will never have another way of retrieving custom actions.
                OpenNotification = null//openNotification //TODO
            });
        }

        public void OnMetadataChanged(RemoteController.MetadataEditor mediaMetadata)
        {
            MediaMetadata = mediaMetadata;
            OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
            {
                MediaTitle= GetStringValue(MetadataKey.Title),
                MediaArtist= GetStringValue(MetadataKey.Artist),
                MediaAlbum= GetStringValue(MetadataKey.Album),
                MediaDuration= GetLongValue(MetadataKey.Duration),
                MediaArtwork= GetBitmap(MediaMetadataEditKey.BitmapKeyArtwork)
            });
        }

        public void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e)
        {
            MediaPlaybackChanged?.Invoke(null, e);
        }

        public void OnMediaMetadataChanged(EventArgs e)
        {
            MediaMetadataChanged?.Invoke(null, (MediaMetadataChangedEventArgs)e);
        }

        public void OnControlsAvailabilityChanged(ControlsAvailabilityChangedEventArgs e)
        {
            ControlsAvailabilityChanged?.Invoke(null, e);
        }

        void TrackProgress(long currentPos)
        {
            currentProgress = currentPos;
            switch (PlaybackState)
            {
                case RemoteControlPlayState.Playing:
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
                TotalProgress = MediaMetadata.GetLong((MediaMetadataEditKey)MetadataKey.Duration, 0)
            });
            if (repeatOptionSet != IMediaEventsPublisher.DontRepeat)
            {
                if (totalProgress - currentProgress <= MillisToRepeat)
                {
                    var mediaControls = MediaControlsBase.Instance;
                    mediaControls?.Pause();
                    mediaControls?.SeekTo(0);
                    mediaControls?.Play();

                    if (repeatOptionSet == IMediaEventsPublisher.RepeatOnce)
                    {
                        repeatOptionSet = IMediaEventsPublisher.DontRepeat;
                    }
                    OnMediaRepeatOptionChanged(repeatOptionSet);
                    Console.WriteLine("REPEATING!!!");
                }
            }
        }

        public void OnMediaProgressChanged(MediaProgressChangedEventArgs e)
        {
            MediaProgressChanged?.Invoke(null, e);
        }

        public void Dispose()
        {
            MediaControlsBase.Instance.MediaEvent -= MusicControlsKitkat_MediaEvent;
            progressTimer.Elapsed -= OnProgressTimerElapsed;

        }

        public void CycleRepeatOption()
        {
            repeatOptionSet++;
            if (repeatOptionSet > IMediaEventsPublisher.RepeatForever)
            {
                repeatOptionSet = IMediaEventsPublisher.DontRepeat;
            }
        }

        public void OnMediaRepeatOptionChanged(int newOption)
        {
            MediaRepeatOptionChanged?.Invoke(null, newOption);
        }

        public MediaSessionSupportedActionsFlags GetSupportedActions()
        {
            var supportedFlags = MediaSessionSupportedActionsFlags.None;
            switch(_remoteControlFlags)
            {
                case RemoteControlFlags.Previous:
                    supportedFlags |= MediaSessionSupportedActionsFlags.SkipToPrevious;
                    break;
                case RemoteControlFlags.Rewind:
                    supportedFlags |= MediaSessionSupportedActionsFlags.Rewind;
                    break;
                case RemoteControlFlags.Play:
                    supportedFlags |= MediaSessionSupportedActionsFlags.Play;
                    break;
                case RemoteControlFlags.PlayPause:
                    supportedFlags |= MediaSessionSupportedActionsFlags.PlayPause;
                    break;
                case RemoteControlFlags.Pause:
                    supportedFlags |= MediaSessionSupportedActionsFlags.Pause;
                    break;
                case RemoteControlFlags.Stop:
                    supportedFlags |= MediaSessionSupportedActionsFlags.Stop;
                    break;
                case RemoteControlFlags.FastForward:
                    supportedFlags |= MediaSessionSupportedActionsFlags.FastForward;
                    break;
                case RemoteControlFlags.Next:
                    supportedFlags |= MediaSessionSupportedActionsFlags.SkipToNext;
                    break;
                case RemoteControlFlags.PositionUpdate:
                    supportedFlags |= MediaSessionSupportedActionsFlags.SeekTo;
                    break;
                case RemoteControlFlags.Rating:
                    supportedFlags |= MediaSessionSupportedActionsFlags.SetRating;
                    break;
            }
            return supportedFlags;
        }

        public string GetStringValue<TKey>(TKey metadataKey)
        {
            MetadataKey key = (MetadataKey)(object)metadataKey;
            return MediaMetadata.GetString((MediaMetadataEditKey)key, string.Empty);
        }
        public long GetLongValue<TKey>(TKey metadataKey) 
        {
            MetadataKey key = (MetadataKey)(object)metadataKey;
            return MediaMetadata.GetLong((MediaMetadataEditKey)key, 0);
        }
        public Bitmap GetBitmap<TKey>(TKey metadataKey)
        {
            MediaMetadataEditKey key = (MediaMetadataEditKey)(object)metadataKey;
            return MediaMetadata.GetBitmap(key, null);
        }

        public AvailableControls SetAvailableControls(MediaSessionSupportedActionsFlags supportedActionsFlags)
        {
           //In Kitkat we don't have any sort of logic regarding the available controls.
           //Though we can set our own, for now we just set all the available controls.
            var availableControls = new AvailableControls();

            if (PlaybackState.HasFlag(RemoteControlPlayState.Buffering))
                availableControls |= AvailableControls.Buffering;


            return availableControls |=
                AvailableControls.PlayPause |
                AvailableControls.SkipToPrevious |
                AvailableControls.SkipToNext |
                AvailableControls.CustomActionOne |
                AvailableControls.CustomActionTwo |
                AvailableControls.Stop |
                AvailableControls.Repeat;
        }
    }
}