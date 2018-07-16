using System;
using System.Threading;
using Android.App;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Views;
using Android.Widget;
using LiveDisplay.Factories;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Music.MediaEventArgs;

namespace LiveDisplay.Fragments
{
    public class MusicFragment : Fragment
    {
        private Jukebox instance;
        private TextView tvTitle, tvArtist, tvAlbum;
        private Button btnSkipPrevious, btnPlayPause, btnSkipNext;
        private LinearLayout lockscreenContainer;
        private SeekBar skbSeekSongTime;
        Window window;
        BitmapDrawable bitmapDrawable;
        private PlaybackStateCode playbackState;
        public override void OnCreate(Bundle savedInstanceState)
        {
            // Create your fragment here
            BindMusicControllerEvents();
            instance = Jukebox.JukeboxInstance();

            
            base.OnCreate(savedInstanceState);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view= inflater.Inflate(Resource.Layout.MusicPlayer, container, false);
            
            //If fails, then WTF? This fragment is called to replace the NotificationFragment when
            //music is playing.
            BindViews(view);
            BindViewEvents();
            //Retrieve current Song playing.
            RetrieveMediaInformation();

            // Use this to return your custom view for this Fragment

            return view;
        }
        private void BindViewEvents()
        {
            btnSkipPrevious.Click += BtnSkipPrevious_Click;
            btnPlayPause.Click += BtnPlayPause_Click;
            btnSkipNext.Click += BtnSkipNext_Click;
            skbSeekSongTime.ProgressChanged += SkbSeekSongTime_ProgressChanged;
        }

        private void SkbSeekSongTime_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            //TODO: Implement seekbar to seek time in the song.
        }

        //Events-
        private void BtnSkipNext_Click(object sender, EventArgs e)
        {
            instance.PlayNext();
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            switch (playbackState)
            {
                //If the media is paused, then Play.
                case PlaybackStateCode.Paused:
                    instance.Play();
                    break;
                //If the media is playing, then pause.
                case PlaybackStateCode.Playing:
                    instance.Pause();
                    break;
            }
            
        }

        private void BtnSkipPrevious_Click(object sender, EventArgs e)
        {
            instance.PlayPrevious();
        }
        //Bind Events Invoked by MusicController
        private void BindMusicControllerEvents()
        {
            //Try to bind Events from MusicController.
            try
            {
                MusicController.MediaPlaybackChanged += MusicController_MediaPlaybackChanged;
                MusicController.MediaMetadataChanged += MusicController_MediaMetadataChanged;
            }
            catch
            {
                Console.WriteLine("Media is not playing");
            }
        }

        private void MusicController_MediaMetadataChanged(object sender, MediaMetadataChangedEventArgs e)
        {
            
            
            tvTitle.Text = e.Title;
            tvAlbum.Text = e.Album;
            tvArtist.Text = e.Artist;

                if (e.AlbumArt != null)
                {
                    bitmapDrawable = new BitmapDrawable(e.AlbumArt);

                    Activity.RunOnUiThread(()=> window.DecorView.Background = bitmapDrawable); 
                }
                //clear bitmap.
            bitmapDrawable = null;
                
        }

        private void MusicController_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedEventArgs e)
        {
            switch (e.PlaybackState)
            {
                case PlaybackStateCode.Paused:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0,Resource.Drawable.ic_play_arrow_white_24dp,  0, 0);
                    playbackState = PlaybackStateCode.Paused;
                    Console.Write("Media is paused");

                    break;
                case PlaybackStateCode.Playing:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0,Resource.Drawable.ic_pause_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Playing;
                    Console.Write("MEdia is playinh");
                    break;
                case PlaybackStateCode.Stopped:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0,Resource.Drawable.ic_play_arrow_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Stopped;
                    Console.Write("Media is Stopped");
                    break;
                default:
                    break;
            }

        }


        //Views
        private void BindViews(View view)
        {
            tvTitle = view.FindViewById<TextView>(Resource.Id.tvSongName);
            tvAlbum = view.FindViewById<TextView>(Resource.Id.tvAlbumName);
            tvArtist = view.FindViewById<TextView>(Resource.Id.tvArtistName);
            //Bind Views to control media.

            btnSkipPrevious = view.FindViewById<Button>(Resource.Id.btnMediaPrevious);
            btnPlayPause = view.FindViewById<Button>(Resource.Id.btnMediaPlayPlause);
            btnSkipNext = view.FindViewById<Button>(Resource.Id.btnMediaNext);
            skbSeekSongTime = view.FindViewById<SeekBar>(Resource.Id.seeksongTime);

            lockscreenContainer = view.FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            window = Activity.Window;
        }
        private void RetrieveMediaInformation()
        {
            OpenSong song = OpenSong.OpenSongInstance();
            
            
            tvTitle.Text = song.Title;
            tvAlbum.Text = song.Album;
            tvArtist.Text = song.Artist;

            if (song.AlbumArt != null)
            {

                BitmapDrawable bitmapDrawable = new BitmapDrawable(song.AlbumArt);
                window.DecorView.Background = bitmapDrawable;
            }
            bitmapDrawable = null;

            switch (song.PlaybackState)
            {
                case PlaybackStateCode.Paused:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0,Resource.Drawable.ic_play_arrow_white_24dp,0,0);
                    break;
                case PlaybackStateCode.Playing:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0,Resource.Drawable.ic_pause_white_24dp,0,0);
                    break;
                case PlaybackStateCode.Stopped:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0,Resource.Drawable.ic_play_arrow_white_24dp,0,0);
                    break;
                default:
                    break;
            }
        }
        public override void OnDestroy()
        {
            tvAlbum = null;
            tvArtist = null;
            tvTitle = null;
            skbSeekSongTime = null;
            MusicController.MediaPlaybackChanged -= MusicController_MediaPlaybackChanged;
            MusicController.MediaMetadataChanged -= MusicController_MediaMetadataChanged;
            base.OnDestroy();
    }
        }
}