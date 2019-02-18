using Android.Media;
using Android.Util;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using System;

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

        public static event EventHandler<MediaPlaybackStateChangedKitkatEventArgs> MediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedKitkatEventArgs> MediaMetadataChanged;

        internal static MusicControllerKitkat GetInstance()
        {
            if (instance == null)
            {
                instance = new MusicControllerKitkat();
            }
            return instance;
        }

        private MusicControllerKitkat()
        {
            Jukebox.MediaEvent += Jukebox_MediaEvent;
        }

        private void Jukebox_MediaEvent(object sender, MediaActionEventArgs e)
        {
            switch (e.MediaActionFlags)
            {
                //case MediaActionFlags.Play:
                //    Play();
                //    break;

                //case MediaActionFlags.Pause:
                //    TransportControls.Pause();
                //    break;

                //case MediaActionFlags.SkipToNext:
                //    TransportControls.SkipToNext();
                //    break;

                //case MediaActionFlags.SkipToPrevious:
                //    TransportControls.SkipToPrevious();
                //    break;

                //case MediaActionFlags.SeekTo:
                //    TransportControls.SeekTo(e.Time);
                //    break;

                //case MediaActionFlags.FastFoward:
                //    TransportControls.FastForward();
                //    break;

                //case MediaActionFlags.Rewind:
                //    TransportControls.Rewind();
                //    break;

                //case Misc.MediaActionFlags.Stop:
                //    TransportControls.Stop();
                //    break;

                //case Misc.MediaActionFlags.RetrieveMediaInformation:
                //    //Send media information.
                //    OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                //    {
                //        MediaMetadata = MediaMetadata
                //    });
                //    //Send Playbackstate of the media.
                //    OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
                //    {
                //        PlaybackState = PlaybackState.State,
                //        CurrentTime = PlaybackState.Position
                //    });

                //    break;

                //default:
                //    break;
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