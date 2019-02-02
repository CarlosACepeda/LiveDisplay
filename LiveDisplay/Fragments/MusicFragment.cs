using Android.App;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Timers;

namespace LiveDisplay.Fragments
{
    public class MusicFragment : Fragment
    {
        private TextView tvTitle, tvArtist, tvAlbum;
        private Button btnSkipPrevious, btnPlayPause, btnSkipNext;
        private LinearLayout musicPlayerContainer;
        private SeekBar skbSeekSongTime;
        private PlaybackStateCode playbackState;
        private Timer timer;
        private ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context));

        #region Fragment Lifecycle

        public override void OnCreate(Bundle savedInstanceState)
        {
            // Create your fragment here
            BindMusicControllerEvents();

            timer = new System.Timers.Timer();
            timer.Interval = 1000; //1 second.
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;

            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.MusicPlayer, container, false);

            BindViews(view);
            BindViewEvents();

            RetrieveMediaInformation();
            return view;
        }

        public override void OnDestroy()
        {
            tvAlbum = null;
            tvArtist = null;
            tvTitle = null;
            skbSeekSongTime = null;
            MusicController.MediaPlaybackChanged -= MusicController_MediaPlaybackChanged;
            MusicController.MediaMetadataChanged -= MusicController_MediaMetadataChanged;
            timer.Elapsed -= Timer_Elapsed;
            timer.Dispose();
            base.OnDestroy();
        }

        #endregion Fragment Lifecycle

        #region Fragment Views events

        private void BindViewEvents()
        {
            btnSkipPrevious.Click += BtnSkipPrevious_Click;
            btnSkipPrevious.LongClick += BtnSkipPrevious_LongClick;
            btnPlayPause.Click += BtnPlayPause_Click;
            btnSkipNext.Click += BtnSkipNext_Click;
            btnSkipNext.LongClick += BtnSkipNext_LongClick;
            skbSeekSongTime.ProgressChanged += SkbSeekSongTime_ProgressChanged;
            skbSeekSongTime.StopTrackingTouch += SkbSeekSongTime_StopTrackingTouch;
            musicPlayerContainer.LongClick += MusicPlayerContainer_LongClick;
        }

        private void BtnSkipNext_LongClick(object sender, View.LongClickEventArgs e)
        {
            Jukebox.FastFoward();
        }

        private void BtnSkipPrevious_LongClick(object sender, View.LongClickEventArgs e)
        {
            Jukebox.Rewind();
        }

        private void MusicPlayerContainer_LongClick(object sender, View.LongClickEventArgs e)
        {
            if (skbSeekSongTime.Visibility == ViewStates.Gone || skbSeekSongTime.Visibility == ViewStates.Invisible)
            {
                skbSeekSongTime.Visibility = ViewStates.Visible;
            }
            else
            {
                skbSeekSongTime.Visibility = ViewStates.Gone;
            }
        }

        private void SkbSeekSongTime_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            //When user stop dragging then seek to the position previously saved in ProgressChangedEvent
            Jukebox.SeekTo(e.SeekBar.Progress);
            skbSeekSongTime.SetProgress(e.SeekBar.Progress, true);
        }

        private void SkbSeekSongTime_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            //This will save the current song time.
            skbSeekSongTime.SetProgress(e.Progress, true);
        }

        private void BtnSkipNext_Click(object sender, EventArgs e)
        {
            Jukebox.SkipToNext();
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            switch (playbackState)
            {
                //If the media is paused, then Play.
                case PlaybackStateCode.Paused:
                    Jukebox.Play();
                    break;
                //If the media is playing, then pause.
                case PlaybackStateCode.Playing:
                    Jukebox.Pause();

                    break;

                default:
                    Jukebox.Stop();
                    break;
            }
        }

        private void BtnSkipPrevious_Click(object sender, EventArgs e)
        {
            Jukebox.SkipToPrevious();
        }

        #endregion Fragment Views events

        #region Subscribing and Reacting to events

        private void BindMusicControllerEvents()
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
            {
                MusicController.MediaPlaybackChanged += MusicController_MediaPlaybackChanged;
                MusicController.MediaMetadataChanged += MusicController_MediaMetadataChanged;
            }
            else
            {
                MusicControllerKitkat.MediaMetadataChanged += MusicControllerKitkat_MediaMetadataChanged;
                MusicControllerKitkat.MediaPlaybackChanged += MusicControllerKitkat_MediaPlaybackChanged;
            }
        }

        private void MusicControllerKitkat_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedKitkatEventArgs e)
        {
            switch (e.PlaybackState)
            {
                case RemoteControlPlayState.Paused:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_play_arrow_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Paused;
                    MoveSeekbar(false);

                    break;

                case RemoteControlPlayState.Playing:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_pause_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Playing;
                    MoveSeekbar(true);

                    break;

                case RemoteControlPlayState.Stopped:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_play_arrow_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Stopped;
                    MoveSeekbar(false);
                    break;

                default:
                    break;
            }
        }

        private void MusicControllerKitkat_MediaMetadataChanged(object sender, MediaMetadataChangedKitkatEventArgs e)
        {
            tvTitle.Text = e.Title;
            tvAlbum.Text = e.Album;
            tvArtist.Text = e.Artist;
            skbSeekSongTime.Max = (int)e.Duration;
            WallpaperPublisher.OnWallpaperChanged(new WallpaperChangedEventArgs
            {
                Wallpaper = new BitmapDrawable(Resources, e.AlbumArt)
            });
            GC.Collect(0);
        }

        private void MusicController_MediaMetadataChanged(object sender, MediaMetadataChangedEventArgs e)
        {
            tvTitle.Text = e.MediaMetadata.GetString(MediaMetadata.MetadataKeyTitle);
            tvAlbum.Text = e.MediaMetadata.GetString(MediaMetadata.MetadataKeyAlbum);
            tvArtist.Text = e.MediaMetadata.GetString(MediaMetadata.MetadataKeyArtist);
            skbSeekSongTime.Max = (int)e.MediaMetadata.GetLong(MediaMetadata.MetadataKeyDuration);
            using (var albumart = e.MediaMetadata.GetBitmap(MediaMetadata.MetadataKeyAlbumArt))
            {
                using (var wallpaper = new BitmapDrawable(Resources, albumart))
                {
                    int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);
                    WallpaperPublisher.OnWallpaperChanged(new WallpaperChangedEventArgs
                    {
                        Wallpaper = wallpaper,
                        OpacityLevel = (short)opacitylevel
                    });
                }
            }
            GC.Collect();
        }

        private void MusicController_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedEventArgs e)
        {
            switch (e.PlaybackState)
            {
                case PlaybackStateCode.Paused:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_play_arrow_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Paused;
                    MoveSeekbar(false);

                    break;

                case PlaybackStateCode.Playing:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_pause_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Playing;
                    MoveSeekbar(true);

                    break;

                case PlaybackStateCode.Stopped:
                    btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_play_arrow_white_24dp, 0, 0);
                    playbackState = PlaybackStateCode.Stopped;
                    MoveSeekbar(false);
                    break;

                default:
                    break;
            }
            skbSeekSongTime.SetProgress((int)e.CurrentTime, true);
        }

        #endregion Subscribing and Reacting to events

        private void BindViews(View view)
        {
            tvTitle = view.FindViewById<TextView>(Resource.Id.tvSongName);
            tvAlbum = view.FindViewById<TextView>(Resource.Id.tvAlbumName);
            tvArtist = view.FindViewById<TextView>(Resource.Id.tvArtistName);

            btnSkipPrevious = view.FindViewById<Button>(Resource.Id.btnMediaPrevious);
            btnPlayPause = view.FindViewById<Button>(Resource.Id.btnMediaPlayPlause);
            btnSkipNext = view.FindViewById<Button>(Resource.Id.btnMediaNext);
            skbSeekSongTime = view.FindViewById<SeekBar>(Resource.Id.seeksongTime);

            musicPlayerContainer = view.FindViewById<LinearLayout>(Resource.Id.musicPlayerContainer);
        }

        private void RetrieveMediaInformation()
        {
            //Syntactic sugar, cause a MediaMetadata and a Mediaplayback event to be fired in the Publisher class.
            //(MusicController class)
            Jukebox.RetrieveMediaInformation();
        }

        private void MoveSeekbar(bool move)
        {
            if (move == true)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            skbSeekSongTime.SetProgress(skbSeekSongTime.Progress + 1000, true);
        }
    }
}