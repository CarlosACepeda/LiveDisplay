using System;
using Android.App;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Music.MediaEventArgs;

namespace LiveDisplay.Fragments
{
    public class MusicFragment : Fragment
    {
        private TextView tvTitle, tvArtist, tvAlbum;
        private Button btnSkipPrevious, btnPlayPause, btnSkipNext;
        Window window;
        public override void OnCreate(Bundle savedInstanceState)
        {
            // Create your fragment here
            BindMusicControllerEvents();

            
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
        }

        //Events-
        private void BtnSkipNext_Click(object sender, EventArgs e)
        {
            //Call Jukebox to Skip Next
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            //Call Jukebox to play/pause the song.
        }

        private void BtnSkipPrevious_Click(object sender, EventArgs e)
        {
            //Call Jukebox to SkipPrevious
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
            BitmapDrawable bitmapDrawable = new BitmapDrawable(e.AlbumArt);
            tvTitle.Text = e.Title;
            tvAlbum.Text = e.Album;
            tvArtist.Text = e.Artist;
            if (bitmapDrawable != null)
            {
                window.DecorView.Background = bitmapDrawable;
                Toast.MakeText(Application.Context, "There is a AlbumArt", ToastLength.Short).Show();
            }
            
        }

        private void MusicController_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedEventArgs e)
        {
            //switch (e.PlaybackState)
            //{
            //    case PlaybackStateCode.Paused:
            //        btnPlayPause.SetBackgroundResource(Resource.Drawable.ic_play_arrow_white_24dp);
            //        break;
            //    case PlaybackStateCode.Playing:
            //        btnPlayPause.SetBackgroundResource(Resource.Drawable.ic_pause_white_24dp);
            //        break;
            //    case PlaybackStateCode.Stopped:
            //        btnPlayPause.SetBackgroundResource(Resource.Drawable.ic_play_arrow_white_24dp);
            //        break;
            //    default:
            //        break;
            //}

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

           window  = Activity.Window;
        }
        private void RetrieveMediaInformation()
        {
                       
            OpenSong song = OpenSong.OpenSongInstance();
            BitmapDrawable bD = new BitmapDrawable(song.AlbumArt);
            tvTitle.Text = song.Title;
            tvAlbum.Text = song.Album;
            tvArtist.Text = song.Artist;
            Activity.Window.DecorView.Background = bD; 
            //switch (song.PlaybackState)
            //{
            //    case PlaybackStateCode.Paused:
            //        btnPlayPause.SetBackgroundResource(Resource.Drawable.ic_play_arrow_white_24dp);
            //        break;
            //    case PlaybackStateCode.Playing:
            //        btnPlayPause.SetBackgroundResource(Resource.Drawable.ic_pause_white_24dp);
            //        break;
            //    case PlaybackStateCode.Stopped:
            //        btnPlayPause.SetBackgroundResource(Resource.Drawable.ic_play_arrow_white_24dp);
            //        break;
            //    default:
            //        break;
            //}
        }
        public override void OnDestroy()
        {
            tvAlbum = null;
            tvArtist = null;
            tvTitle = null;
            base.OnDestroy();
        }
    }
}