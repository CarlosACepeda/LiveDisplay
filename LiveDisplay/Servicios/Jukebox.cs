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

namespace LiveDisplay.Servicios
{
    /// <summary>
    /// Nice name, isn't it?, this class is for controlling the Multimedia that is currently playing
    /// Play/pause/forward/rewind, etc.
    /// for Lollipop and beyond
    /// </summary>
    class Jukebox
    {
        private static Jukebox jukeboxInstance;
        private Android.Media.Session.MediaController.TransportControls transportControls;
        private Jukebox(Android.Media.Session.MediaController.TransportControls transportControls)
        {
            this.transportControls = transportControls;
        }
        public static Jukebox JukeboxInstance(Android.Media.Session.MediaController.TransportControls transportControls)
        {
            if (jukeboxInstance == null)
            {
                jukeboxInstance = new Jukebox(transportControls);
                
            }

            return jukeboxInstance;
        }

        public void Play()
        {
            transportControls.Play();
        }
        public void Pause()
        {
            transportControls.Pause();
        }
        public void PlayNext()
        {
            transportControls.SkipToNext();
        }
        public void PlayPrevious()
        {
            transportControls.SkipToPrevious();
        }
        public void SeekTo(long time)
        {
            transportControls.SeekTo(time);
            
        }
        public void FastFoward()
        {
            transportControls.FastForward();
        }
        public void Rewind()
        {
            transportControls.Rewind();
        }
    }
}