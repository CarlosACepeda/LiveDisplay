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

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// Nice name, isn't it?, this class is for controlling the Multimedia that is currently playing
    /// Play/pause/forward/rewind, etc.
    /// for Lollipop and beyond
    /// </summary>
    class Jukebox
    {
        
        public static void Play()
        {
            transportControls.Play();
        }
        public static void Pause()
        {
            transportControls.Pause();
        }
        public static void PlayNext()
        {
            transportControls.SkipToNext();
        }
        public static void PlayPrevious()
        {
            transportControls.SkipToPrevious();
        }
        public static void SeekTo(long time)
        {
            transportControls.SeekTo(time);
            
        }
        public static void FastFoward()
        {
            transportControls.FastForward();
        }
        public static void Rewind()
        {
            transportControls.Rewind();
        }

        internal static void SkipNext()
        {
            throw new NotImplementedException();
        }

        internal static void Stop()
        {
            throw new NotImplementedException();
        }
    }
}