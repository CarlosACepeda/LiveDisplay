using Android.App;
using Android.Media;
using Android.Util;
using Android.Views;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media.MediaEventArgs;
using System;
using System.Collections.Generic;


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
        private int optionSet;
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
        public long CurrentMediaPosition { get; set; }


        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;

        public static event EventHandler<MediaProgressChangedEventArgs> MediaProgressChanged;
        public static event EventHandler<int> MediaRepeatOptionChanged;

        private MediaEventsPublisherKitkat(RemoteController remoteController)
        {
            MediaControlsKitkat.GetInstance().MediaEvent += MusicControlsKitkat_MediaEvent;
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
                    OnMediaRepeatOptionChanged(optionSet);
                    break;
                case MediaActionFlags.OpenRelatedActivity:
                    OpenRelatedActivity();
                    break;
                default:
                    break;
            }
            Console.WriteLine("Sending Mediaplayback manually, cuz apparently after sending the key event the media the RemoteController doesn't react");
            //Send Playbackstate of the media Manually
            OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
            {
                PlaybackStateKitkat = simulatedState,
                CurrentTime = TransportControls.EstimatedMediaPosition,
                RepeatOptionSet= optionSet
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
                SupportedActions= GetSupportedActions()
            });
        }

        public void OnMetadataChanged(RemoteController.MetadataEditor mediaMetadata)
        {
            MediaMetadata = mediaMetadata;
            OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
            {
                MediaMetadataKitkat= mediaMetadata
            });;
        }

        public void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e)
        {
            MediaPlaybackChanged?.Invoke(null, e);
        }

        public void OnMediaMetadataChanged(EventArgs e)
        {
            MediaMetadataChanged?.Invoke(null, (MediaMetadataChangedEventArgs)e);
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

        public void Dispose()
        {
            MediaControlsKitkat.GetInstance().MediaEvent -= MusicControlsKitkat_MediaEvent;
            progressTimer.Elapsed -= OnProgressTimerElapsed;

        }

        public void CycleRepeatOption()
        {
            optionSet++;
            if (optionSet > IMediaEventsPublisher.RepeatForever)
            {
                optionSet = IMediaEventsPublisher.DontRepeat;
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
    }
}