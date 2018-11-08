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
    /// This class receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks 
    /// For Lollipop and beyond.
    /// </summary>
    internal class MusicController : MediaController.Callback
    {
    #region events
        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged;
        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged;
        #endregion
        
        #region Class members
        public PlaybackState PlaybackState {get;set;}|
        public TransportControls TransportControls {get; set;}
        public MediaMetadata MediaMetadata {get; set;}
        
        #endregion
        
        private OpenSong song;
        Bitmap bitmap;
        public MusicController()
        {

        }   
        
        public override void OnPlaybackStateChanged(PlaybackState state)
        {
        PlaybackState= state;
            song.PlaybackState = state.State;
            song.CurrentPosition = (int)(state.Position / 1000);
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
            int size = (int)Application.Context.Resources.GetDimension(Resource.Dimension.albumartsize);
            if (metadata != null)
            {

            }
            try
            {
                bitmap = Bitmap.CreateScaledBitmap(metadata.GetBitmap(MediaMetadata.MetadataKeyAlbumArt), size, size, true);
            }
            catch
            {
                //There is not albumart
            }
            try
            {
                song.Title = metadata.GetText(MediaMetadata.MetadataKeyTitle);
                song.Artist = metadata.GetText(MediaMetadata.MetadataKeyArtist);
                song.Album = metadata.GetText(MediaMetadata.MetadataKeyAlbum);
                song.AlbumArt = bitmap;
                //

                OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                {
                    Artist = metadata.GetText(MediaMetadata.MetadataKeyArtist),
                    Title = metadata.GetText(MediaMetadata.MetadataKeyTitle),
                    Album = metadata.GetText(MediaMetadata.MetadataKeyAlbum),
                    AlbumArt = bitmap

                });
                bitmap = null;
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
