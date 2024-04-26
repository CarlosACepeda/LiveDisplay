using LiveDisplay.Misc;
using LiveDisplay.Services.Music.MediaEventArgs;
using System;

namespace LiveDisplay.Services.Music
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

<<<<<<<< HEAD:LiveDisplay/Services/Music/MusicControlsLollipop.cs
        public void RetrieveMediaInformation()
========
        internal static void Replay()
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.Replay
            });
        }

        internal static void RetrieveMediaInformation()
>>>>>>>> a7857b231af0cd4428f4a29a514de46bb05768de:LiveDisplay/Services/Music/Jukebox.cs
        {
            OnMediaEvent(new MediaActionEventArgs
            {
                MediaActionFlags = MediaActionFlags.RetrieveMediaInformation
            });
        }
    }
}