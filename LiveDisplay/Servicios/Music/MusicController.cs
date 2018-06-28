using System;
using System.Collections.Generic;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using LiveDisplay.Servicios.Music.MediaEventArgs;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks 
    /// For Lollipop and beyond.
    /// </summary>
    internal class MusicController : MediaController.Callback
    {
        private static MusicController instance;
        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;
        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;
        private OpenSong song = OpenSong.OpenSongInstance();
        private MusicController()
        {

        }
        public static MusicController MusicControllerInstance()
        {
            if (instance == null)
            {
                instance=new MusicController();
            }
            return instance;
            
        }
        public override void OnPlaybackStateChanged(PlaybackState state)
        {
            song.PlaybackState = state.State;
            //Estado del playback:
            //Pausado, Comenzado, Avanzando, Retrocediendo, etc.    
                OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
                {
                PlaybackState = state.State
                });
            base.OnPlaybackStateChanged(state);
        }
        public override void OnMetadataChanged(MediaMetadata metadata)
        {
            
            song.Title = metadata.GetText(MediaMetadata.MetadataKeyTitle);
            song.Artist = metadata.GetText(MediaMetadata.MetadataKeyArtist);
            song.Album = metadata.GetText(MediaMetadata.MetadataKeyAlbum);

            OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                    {
                        Artist = metadata.GetText(MediaMetadata.MetadataKeyArtist),
                        Title = metadata.GetText(MediaMetadata.MetadataKeyTitle),
                        Album = metadata.GetText(MediaMetadata.MetadataKeyAlbum),
                        AlbumArt = metadata.GetBitmap(MediaMetadata.MetadataKeyAlbumArt)

                    });
            
            //Datos de la Media que se está reproduciendo.            

            base.OnMetadataChanged(metadata);
        }
        //Raise Events:
        protected virtual void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e)
        {
            MediaPlaybackChanged?.Invoke(this, e);
        }
        protected virtual void OnMediaMetadataChanged(MediaMetadataChangedEventArgs e)
        {
            MediaMetadataChanged?.Invoke(this, e);
        }
    }
}