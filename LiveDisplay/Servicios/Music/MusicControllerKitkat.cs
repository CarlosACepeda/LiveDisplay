using Android.Media;
using Android.Util;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using System;
using Android.Views;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks
    /// For Kitkat only.
    /// </summary>

    internal class MusicControllerKitkat : IDisposable
    {
        private static MusicControllerKitkat instance;

        public static RemoteControlPlayState MusicStatus { get; private set; }
        public RemoteControlPlayState PlaybackState { get; set; }
        public RemoteController.MetadataEditor MediaMetadata { get; set; }
        public RemoteController TransportControls { get; set; }
        public long CurrentMediaPosition { get; set; }

        public static event EventHandler<MediaPlaybackStateChangedKitkatEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedKitkatEventArgs> MediaMetadataChanged;

        internal static MusicControllerKitkat GetInstance(RemoteController remoteController)
        {
            if (instance == null)
            {
                instance = new MusicControllerKitkat(remoteController);
            }
            return instance;
        }

        private MusicControllerKitkat(RemoteController remoteController)
        {
            JukeboxKitkat.MediaEvent += Jukebox_MediaEvent;
            TransportControls = remoteController;
        }

        private void Jukebox_MediaEvent(object sender, MediaActionEventArgs e)
        {
            switch (e.MediaActionFlags)
            {
                case MediaActionFlags.Play:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaPlay));
                    break;

                case MediaActionFlags.Pause:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaPause));
                    break;

                case MediaActionFlags.SkipToNext:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaNext));
                    break;

                case MediaActionFlags.SkipToPrevious:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaPrevious));
                    break;

                case MediaActionFlags.SeekTo:
                    TransportControls.SeekTo(e.Time);
                    break;

                case MediaActionFlags.FastFoward:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaFastForward));
                    break;

                case MediaActionFlags.Rewind:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaRewind));
                    break;

                case MediaActionFlags.Stop:
                    TransportControls.SendMediaKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.MediaStop));
                    break;

                case MediaActionFlags.RetrieveMediaInformation:
                    //Send media information.
                    OnMediaMetadataChanged(new MediaMetadataChangedKitkatEventArgs
                    {
                        Title = MediaMetadata.GetString((MediaMetadataEditKey)MetadataKey.Title, ""),
                        Artist = MediaMetadata.GetString((MediaMetadataEditKey)MetadataKey.Artist, ""),
                        Album = MediaMetadata.GetString((MediaMetadataEditKey)MetadataKey.Album, ""),
                        AlbumArt = MediaMetadata.GetBitmap(MediaMetadataEditKey.BitmapKeyArtwork, null),
                        Duration = MediaMetadata.GetLong((MediaMetadataEditKey)MetadataKey.Duration, 0)
                    });
                    //Send Playbackstate of the media.
                    OnMediaPlaybackChanged(new MediaPlaybackStateChangedKitkatEventArgs
                    {
                        PlaybackState = PlaybackState,
                        CurrentTime = CurrentMediaPosition,
                    });

                    break;

                default:
                    break;
            }
        }

        public void OnPlaybackStateChanged(RemoteControlPlayState state)
        {
            PlaybackState = state;
            MusicStatus = state;
            Log.Info("LiveDisplay", "Music state is" + state);
            OnMediaPlaybackChanged(new MediaPlaybackStateChangedKitkatEventArgs
            {
                PlaybackState = state
            });
        }

        public void OnMetadataChanged(RemoteController.MetadataEditor mediaMetadata)
        {
            MediaMetadata = mediaMetadata;
            OnMediaMetadataChanged(new MediaMetadataChangedKitkatEventArgs
            {
                Title = mediaMetadata.GetString((MediaMetadataEditKey)MetadataKey.Title, ""),
                Artist = mediaMetadata.GetString((MediaMetadataEditKey)MetadataKey.Artist, ""),
                Album = mediaMetadata.GetString((MediaMetadataEditKey)MetadataKey.Album, ""),
                AlbumArt = mediaMetadata.GetBitmap(MediaMetadataEditKey.BitmapKeyArtwork, null),
                Duration = mediaMetadata.GetLong((MediaMetadataEditKey)MetadataKey.Duration, 0)
            });
        }

        #region Raising events.

        protected virtual void OnMediaPlaybackChanged(MediaPlaybackStateChangedKitkatEventArgs e)
        {
            MediaPlaybackChanged?.Invoke(this, e);
        }

        protected virtual void OnMediaMetadataChanged(MediaMetadataChangedKitkatEventArgs e)
        {
            MediaMetadataChanged?.Invoke(this, e);
        }

        #endregion Raising events.

        public void Dispose()
        {
            Jukebox.MediaEvent -= Jukebox_MediaEvent;
        }
    }
}