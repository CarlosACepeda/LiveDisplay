using Android.Media;
using Android.Util;
using Android.Views;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using LiveDisplay.Servicios.Widget;
using System;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks
    /// For Kitkat only.
    /// </summary>

#pragma warning disable CS0618 // Type or member is obsolete
    internal class MediaEventsPublisherKitkat : IMediaEventsPublisher, IDisposable
    {
        private static MediaEventsPublisherKitkat instance;
        public RemoteControlPlayState PlaybackState { get; set; }
        public RemoteController.MetadataEditor MediaMetadata { get; set; }
        public RemoteController TransportControls { get; set; }
        public long CurrentMediaPosition { get; set; }


        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;

        public event EventHandler MusicPlaying;

        public event EventHandler MusicPaused;

        private MediaEventsPublisherKitkat(RemoteController remoteController)
        {
            MusicControlsKitkat.GetInstance().MediaEvent += MusicControlsKitkat_MediaEvent;
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
                    //Send media information.
                    OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                    {
                        MediaMetadataKitkat= MediaMetadata
                    });
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
            });
        }

        public void OnPlaybackStateChanged(RemoteControlPlayState state)
        {
            PlaybackState = state;
            Log.Info("LiveDisplay", "Music state is: " + state);
            OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
            {
                PlaybackStateKitkat = state
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

        public void Dispose()
        {
            MusicControlsKitkat.GetInstance().MediaEvent -= MusicControlsKitkat_MediaEvent;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}