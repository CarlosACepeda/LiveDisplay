using System;
using System.Collections.Generic;
using Android.Media;
using Android.Media.Session;
using Android.OS;

namespace LiveDisplay.Servicios
{
    /// <summary>
    /// This class receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks 
    /// For Lollipop and beyond.
    /// </summary>
    internal class MusicController : MediaController.Callback
    {
        public override void OnSessionEvent(string e, Bundle extras)
        {
            Console.WriteLine("OnSessionEvent called");
            Console.WriteLine(e);
            base.OnSessionEvent(e, extras);
        }
        public override void OnPlaybackStateChanged(PlaybackState state)
        {
            //Estado del playback:
            //Pausado, Comenzado, Avanzando, Retrocediendo, etc.
            base.OnPlaybackStateChanged(state);
        }
        public override void OnMetadataChanged(MediaMetadata metadata)
        {
            //Datos de la Media que se está reproduciendo.
            Console.WriteLine("Artist of the song is: " + metadata.GetText(MediaMetadata.MetadataKeyArtist));
            Console.WriteLine("Name of the song is: " + metadata.GetText(MediaMetadata.MetadataKeyTitle));
            base.OnMetadataChanged(metadata);

        }
        public override void OnSessionDestroyed()
        {
            Console.WriteLine("OnSessionDestroyed");
            base.OnSessionDestroyed();
        }
    }
}