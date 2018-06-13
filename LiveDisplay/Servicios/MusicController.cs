using System;
using System.Collections.Generic;
using Android.Media;
using Android.Media.Session;

namespace LiveDisplay.Servicios
{
    internal class MusicController : MediaController.Callback
    {
        public override void OnMetadataChanged(MediaMetadata metadata)
        {
            Console.WriteLine("Artist of the song is: " + metadata.GetText(MediaMetadata.MetadataKeyArtist));
            Console.WriteLine("Name of the song is: " + metadata.GetText(MediaMetadata.MetadataKeyTitle));
            base.OnMetadataChanged(metadata);

        }
    }
}