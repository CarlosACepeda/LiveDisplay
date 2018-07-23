using Android.Graphics;
using Android.Media.Session;

namespace LiveDisplay.Servicios.Music
{
    internal class OpenSong
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public Bitmap AlbumArt { get; set; }
        public PlaybackStateCode PlaybackState { get; set; }
        public int Duration { get; internal set; }
        public int CurrentPosition { get; set; }

        private static OpenSong instance;
        private OpenSong()
        {

        }
        public static OpenSong OpenSongInstance()
        {
            if (instance == null)
            {
                instance = new OpenSong();
            }
            return instance;
        }
    }
}