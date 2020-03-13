using Android.App;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Threading;
using System.Timers;
using Fragment = Android.App.Fragment;
using Timer = System.Timers.Timer;

namespace LiveDisplay.Fragments
{
    public class MusicFragment : Fragment
    {
        private TextView tvTitle, tvArtist, tvAlbum;
        private Button btnSkipPrevious, btnPlayPause, btnSkipNext;
        private LinearLayout musicPlayerContainer;
        private SeekBar skbSeekSongTime;
        private PlaybackStateCode playbackState;
        private PendingIntent activityIntent; //A Pending intent if available to start the activity associated with this music fragent.
        BitmapDrawable CurrentAlbumArt; 


        private Timer timer;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        #region Fragment Lifecycle

        public override void OnCreate(Bundle savedInstanceState)
        {
            BindMusicControllerEvents();

            timer = new Timer
            {
                Interval = 1000 //1 second.
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;


            WallpaperPublisher.CurrentWallpaperCleared += WallpaperPublisher_CurrentWallpaperHasBeenCleared;

            base.OnCreate(savedInstanceState);
        }

        private void WallpaperPublisher_CurrentWallpaperHasBeenCleared(object sender, CurrentWallpaperClearedEventArgs e)
        {
            if (e.PreviousWallpaperPoster == WallpaperPoster.MusicPlayer)
            {
                int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, 255);
                int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, 1); //Never used (for now)

                if (configurationManager.RetrieveAValue(ConfigurationParameters.ShowAlbumArt))
                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        Wallpaper = CurrentAlbumArt,
                        OpacityLevel = (short)opacitylevel,
                        BlurLevel = 0, //Causes a crash That currently I cant debug, damn, thats why is 0. (No blur) and ignoring the value the used have setted.
                        WallpaperPoster = WallpaperPoster.MusicPlayer //We must nutify WallpaperPublisher who is posting the wallpaper, otherwise the wallpaper will be ignored.

                    });

            }
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
            MusicControllerKitkat.MediaMetadataChanged -= MusicControllerKitkat_MediaMetadataChanged;
            MusicControllerKitkat.MediaPlaybackChanged -= MusicControllerKitkat_MediaPlaybackChanged;
            WallpaperPublisher.CurrentWallpaperCleared -= WallpaperPublisher_CurrentWallpaperHasBeenCleared;
            WallpaperPublisher.ReleaseWallpaper();
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
            musicPlayerContainer.Click += MusicPlayerContainer_Click;
        }

        private void MusicPlayerContainer_Click(object sender, EventArgs e)
        {
            try { activityIntent.Send(); }
            catch { Log.Info("LiveDisplay", "Failed to send the Music pending intent"); }
        }

        private void BtnSkipNext_LongClick(object sender, View.LongClickEventArgs e)
        {
            bool isNotKitkat = Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch;
            if (isNotKitkat)
            {
                Jukebox.FastFoward();
            }
            else
            {
                JukeboxKitkat.FastFoward();
            }
        }

        private void BtnSkipPrevious_LongClick(object sender, View.LongClickEventArgs e)
        {
            bool isNotKitkat = Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch;
            if (isNotKitkat)
            {
                Jukebox.Rewind();
            }
            else
            {
                JukeboxKitkat.Rewind();
            }
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
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
            {
                //When user stop dragging then seek to the position previously saved in ProgressChangedEvent
                Jukebox.SeekTo(e.SeekBar.Progress);
                if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
                {
                    skbSeekSongTime.SetProgress(e.SeekBar.Progress, true);
                }
                else
                {
                    skbSeekSongTime.Progress = e.SeekBar.Progress;
                }
            }
            else
            {
                JukeboxKitkat.SeekTo(e.SeekBar.Progress);
                skbSeekSongTime.Progress = e.SeekBar.Progress;
            }
        }

        private void SkbSeekSongTime_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            //This will save the current song time.
            bool isNotMarshmallow = Build.VERSION.SdkInt > BuildVersionCodes.M;
            if (isNotMarshmallow)
            {
                skbSeekSongTime.SetProgress(e.Progress, true);
            }
            else
            {
                skbSeekSongTime.Progress = e.Progress;
            }
        }

        private void BtnSkipNext_Click(object sender, EventArgs e)
        {
            bool isNotKitkat = Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch;
            if (isNotKitkat)
            {
                Jukebox.SkipToNext();
            }
            else
            {
                JukeboxKitkat.SkipToNext();
            }
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            bool isNotKitkat = Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch;
            switch (playbackState)
            {
                //If the media is paused, then Play.
                case PlaybackStateCode.Paused:
                    if (isNotKitkat)
                    {
                        Jukebox.Play();
                    }
                    else
                    {
                        JukeboxKitkat.Play();
                    }
                    break;
                //If the media is playing, then pause.
                case PlaybackStateCode.Playing:
                    if (isNotKitkat)
                    {
                        Jukebox.Pause();
                    }
                    else
                    {
                        JukeboxKitkat.Pause();
                    }

                    break;

                default:
                    if (isNotKitkat)
                    {
                        Jukebox.Stop();
                    }
                    else
                    {
                        JukeboxKitkat.Stop();
                    }
                    break;
            }
        }

        private void BtnSkipPrevious_Click(object sender, EventArgs e)
        {
            bool isNotKitkat = Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch;
            if (isNotKitkat)
            {
                Jukebox.SkipToPrevious();
            }
            else
            {
                JukeboxKitkat.SkipToPrevious();
            }
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
            Activity?.RunOnUiThread(() =>
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
            });
        }

        private void MusicControllerKitkat_MediaMetadataChanged(object sender, MediaMetadataChangedKitkatEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                tvTitle.Text = e.Title;
                tvAlbum.Text = e.Album;
                tvArtist.Text = e.Artist;
                skbSeekSongTime.Max = (int)e.Duration;
                int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, 255);
                int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, 1); //Never used (for now)
                CurrentAlbumArt = new BitmapDrawable(Resources, e.AlbumArt);

                if (configurationManager.RetrieveAValue(ConfigurationParameters.ShowAlbumArt))
                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        Wallpaper = new BitmapDrawable(Resources, e.AlbumArt),
                        OpacityLevel = (short)opacitylevel,
                        BlurLevel = 0, //Causes a crash That currently I cant debug, damn, thats why is 0. (No blur) and ignoring the value the used have setted.
                        WallpaperPoster = WallpaperPoster.MusicPlayer //We must nutify WallpaperPublisher who is posting the wallpaper, otherwise it'll be ignored.

                    });
                GC.Collect(0);
            });
        }

        private void MusicController_MediaMetadataChanged(object sender, MediaMetadataChangedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                activityIntent = e.ActivityIntent;
                tvTitle.Text = e.MediaMetadata.GetString(MediaMetadata.MetadataKeyTitle);
                tvAlbum.Text = e.MediaMetadata.GetString(MediaMetadata.MetadataKeyAlbum);
                tvArtist.Text = e.MediaMetadata.GetString(MediaMetadata.MetadataKeyArtist);
                skbSeekSongTime.Max = (int)e.MediaMetadata.GetLong(MediaMetadata.MetadataKeyDuration);
                ThreadPool.QueueUserWorkItem(m =>
                {
                    var albumart = e.MediaMetadata.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);
                    var wallpaper = new BitmapDrawable(Activity.Resources, albumart);
                    int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, 255);
                    int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, 1); //Never used (for now)
                    CurrentAlbumArt = wallpaper;

                    if (configurationManager.RetrieveAValue(ConfigurationParameters.ShowAlbumArt))
                        WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                        {
                            Wallpaper= wallpaper,
                            OpacityLevel = (short)opacitylevel,
                            BlurLevel = 0, //Causes a crash That currently I cant debug, damn, thats why is 0. (No blur) and ignoring the value the used have setted.
                            WallpaperPoster= WallpaperPoster.MusicPlayer //We must nutify WallpaperPublisher who is posting the wallpaper, otherwise it'll be ignored.
                        });
                });
            });
        }

        private void MusicController_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
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
            });
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
            //Or in Catcher class, if its Kitkat.
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
            {
                Jukebox.RetrieveMediaInformation();
            }
            else
            {
                JukeboxKitkat.RetrieveMediaInformation();
            }
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
            if (Build.VERSION.SdkInt > BuildVersionCodes.M)
            {
                skbSeekSongTime.SetProgress(skbSeekSongTime.Progress + 1000, true);
            }
            else
            {
                skbSeekSongTime.Progress += 1000;
            }
        }
    }
}