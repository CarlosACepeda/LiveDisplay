using Android.Media;
using Android.Runtime;
using System;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// This class receives Callbacks with Media metadata and other information about media playing.
    /// This class is registered in Catcher to receive callbacks
    /// For Kitkat only.
    /// </summary>
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos

    internal class MusicControllerKitkat : Java.Lang.Object, RemoteController.IOnClientUpdateListener
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
    {
        public void OnClientChange(bool clearing)
        {
            Console.WriteLine("OnClientChange called");
        }

#pragma warning disable CS0618 // El tipo o el miembro están obsoletos

        public void OnClientMetadataUpdate(RemoteController.MetadataEditor metadataEditor)
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
        {
            Console.WriteLine("OnClientMetadataUpdate called");
        }

        public void OnClientPlaybackStateUpdateSimple([GeneratedEnum] RemoteControlPlayState stateSimple)
        {
            Console.WriteLine("OnClientPlaybackStateUpdateSimple called");
        }

        public void OnClientPlaybackStateUpdate([GeneratedEnum] RemoteControlPlayState state, long stateChangeTimeMs, long currentPosMs, float speed)
        {
            Console.WriteLine("OnClientPlaybackStateUpdate called");
        }

        public void OnClientTransportControlUpdate([GeneratedEnum] RemoteControlFlags transportControlFlags)
        {
            Console.WriteLine("OnClientTransportControlUpdate called");
        }
    }
}