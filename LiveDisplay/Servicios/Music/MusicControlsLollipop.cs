using LiveDisplay.Misc;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using System;

namespace LiveDisplay.Servicios.Music
{
    /// <summary>
    /// Nice name, isn't it?, this class is for controlling the Multimedia that is currently playing
    /// Play/pause/forward/rewind, etc.
    /// for Lollipop and beyond
    /// </summary>
    internal class MusicControlsLollipop: MusicControlsBase, IMusicControls
    {
        private static MusicControlsLollipop _instance;
        public static MusicControlsLollipop GetInstance()
        {
            _instance ??= new MusicControlsLollipop();
            return _instance;
        }
        private MusicControlsLollipop()
        {
        }
        public void Play()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Play
            });
        }

        public void Pause()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Pause
            });
        }

        public void SkipToPrevious()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.SkipToPrevious
            });
        }

        public void SeekTo(long time)
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.SeekTo,
                Time = time
            });
        }

        public void FastForward()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.FastFoward
            });
        }

        public void Rewind()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Rewind
            });
        }

        public void SkipToNext()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.SkipToNext
            });
        }

        public void Stop()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Stop
            });
        }

        public void RetrieveMediaInformation()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.RetrieveMediaInformation
            });
        }
    }
}