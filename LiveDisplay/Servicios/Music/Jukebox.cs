using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Music.MediaEventArgs;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// Nice name, isn't it?, this class is for controlling the Multimedia that is currently playing
    /// Play/pause/forward/rewind, etc.
    /// for Lollipop and beyond
    /// </summary>
    class Jukebox
    {
        public static event EventHandler<MediaActionEventArgs> MediaEvent;
        
        public static void Play()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags= MediaActionFlags.Play
            });
        }
        public static void Pause()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Pause
            });
        }

        public static void SkipToPrevious()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.SkipToPrevious
            });
        }
        public static void SeekTo(long time)
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.SeekTo,
                Time= time

            });

        }
        public static void FastFoward()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.FastFoward
            });
        }
        public static void Rewind()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Rewind
            });
        }

        internal static void SkipToNext()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.SkipToNext
            });
        }

        internal static void Stop()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Stop
            });
        }
         internal static void RetrieveMediaInformation()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.RetrieveMediaInformation
            });
        }
        static void OnMediaEvent(MediaActionEventArgs e)
        {
            MediaEvent?.Invoke(null,e);
        }
    }
}