using Android.App;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Keyguard;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Wallpaper;
using LiveDisplay.Servicios.Widget;
using System;
using System.Threading;
using Fragment = AndroidX.Fragment.App.Fragment;
using Java.Lang;

namespace LiveDisplay.Fragments
{
    public class MusicFragment : Fragment
    {
        private TextView tvTitle, tvArtist, tvAlbum, sourceApp, playbackstatus;
        private Button btnSkipPrevious, btnPlayPause, btnSkipNext, btnLaunchNotification;
        private LinearLayout maincontainer;
        private SeekBar skbSeekSongTime;
        private PlaybackStateCode playbackState;
        private PendingIntent activityIntent; //A Pending intent if available to start the activity associated with this music fragent.
        private BitmapDrawable CurrentAlbumArt;
        private OpenNotification openNotification; //Used if the button launch Notification is used.
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        private bool timeoutStarted = false;
        private bool initForFirstTime= true;
        private Handler handler;
        private Runnable runnable;
        #region Fragment Lifecycle

        public override void OnCreate(Bundle savedInstanceState)
        {
             handler = new Handler();
             runnable = new Runnable(MoveSeekbar);

            WallpaperPublisher.CurrentWallpaperCleared += WallpaperPublisher_CurrentWallpaperHasBeenCleared;
            WidgetStatusPublisher.OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;

            base.OnCreate(savedInstanceState);
        }

        private void WidgetStatusPublisher_OnWidgetStatusChanged(object sender, WidgetStatusEventArgs e)
        {
            if (e.WidgetName == "MusicFragment")
            {
                if (e.Show)
                {
                    if (maincontainer != null)
                    {
                        if (initForFirstTime==true)
                        {
                            RetrieveMediaInformation(); //Retrieving media information is when the music widget has never been used before.
                            //so it needs information to fill its views.
                            initForFirstTime = false;
                        }
                        maincontainer.Visibility = ViewStates.Visible;
                    }
                }
                else
                {
                    if (maincontainer != null)
                    {
                        maincontainer.Visibility = ViewStates.Invisible;
                        //Also release Wallpaper, if holded.
                        WallpaperPublisher.ReleaseWallpaper();
                    }
                }
            }
            if (e.WidgetName == "NotificationFragment")
            {
                if (e.Show)
                {
                    if (maincontainer != null)
                        maincontainer.Visibility = ViewStates.Invisible;

                }
                else
                {
                    if (maincontainer != null && WidgetStatusPublisher.CurrentActiveWidget== "MusicFragment")
                        maincontainer.Visibility = ViewStates.Visible;
                }
            }

        }

        private void WallpaperPublisher_CurrentWallpaperHasBeenCleared(object sender, CurrentWallpaperClearedEventArgs e)
        {
            if (e.PreviousWallpaperPoster == WallpaperPoster.MusicPlayer)
            {
                int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, ConfigurationParameters.DefaultAlbumartOpacityLevel);
                int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, ConfigurationParameters.DefaultAlbumartBlurLevel); 

                if (configurationManager.RetrieveAValue(ConfigurationParameters.ShowAlbumArt))
                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        Wallpaper = CurrentAlbumArt,
                        OpacityLevel = (short)opacitylevel,
                        BlurLevel = (short)blurLevel,
                        WallpaperPoster = WallpaperPoster.MusicPlayer //We must nutify WallpaperPublisher who is posting the wallpaper, otherwise the wallpaper will be ignored.
                    });
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //View view = inflater.Inflate(Resource.Layout.MusicPlayer, container, false);
            View view = inflater.Inflate(Resource.Layout.MusicPlayer2, container, false);

            BindViews(view);
            BindViewEvents();
            BindMusicControllerEvents();            
            
            return view;
        }
        public override void OnDestroyView()
        {
            UnbindMusicControllerEvents();
            WallpaperPublisher.ReleaseWallpaper();
            
            UnbindViewEvents();

            base.OnDestroyView();
        }
        public override void OnPause()
        {
            base.OnPause();
        }
        public override void OnResume()
        {
            if(WidgetStatusPublisher.CurrentActiveWidget == "MusicFragment" && initForFirstTime==true)
            {
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "MusicFragment", Active = true });
            }
            base.OnResume();
        }


        private void UnbindMusicControllerEvents()
        {
            MusicController.MediaPlaybackChanged -= MusicController_MediaPlaybackChanged;
            MusicController.MediaMetadataChanged -= MusicController_MediaMetadataChanged;
            MusicControllerKitkat.MediaMetadataChanged -= MusicControllerKitkat_MediaMetadataChanged;
            MusicControllerKitkat.MediaPlaybackChanged -= MusicControllerKitkat_MediaPlaybackChanged;
            WallpaperPublisher.CurrentWallpaperCleared -= WallpaperPublisher_CurrentWallpaperHasBeenCleared;
        }

        public override void OnDestroy()
        {
            tvAlbum = null;
            tvArtist = null;
            tvTitle = null;
            skbSeekSongTime = null;
            bool isWidgetActive = WidgetStatusPublisher.CurrentActiveWidget == "MusicFragment";
            WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "MusicFragment", Active = isWidgetActive });
            WidgetStatusPublisher.OnWidgetStatusChanged -= WidgetStatusPublisher_OnWidgetStatusChanged;
            //UnbindViews();
            initForFirstTime = false;
            base.OnDestroy();
        }

        #endregion Fragment Lifecycle

        #region Fragment Views events

        private void UnbindViewEvents()
        {
            btnSkipPrevious.Click -= BtnSkipPrevious_Click;
            btnSkipPrevious.LongClick -= BtnSkipPrevious_LongClick;
            btnPlayPause.Click -= BtnPlayPause_Click;
            btnSkipNext.Click -= BtnSkipNext_Click;
            btnSkipNext.LongClick -= BtnSkipNext_LongClick;
            skbSeekSongTime.ProgressChanged -= SkbSeekSongTime_ProgressChanged;
            skbSeekSongTime.StopTrackingTouch -= SkbSeekSongTime_StopTrackingTouch;
            maincontainer.LongClick -= MusicPlayerContainer_LongClick;
            maincontainer.Click -= MusicPlayerContainer_Click;
        }
        private void BindViewEvents()
        {
            btnSkipPrevious.Click += BtnSkipPrevious_Click;
            btnSkipPrevious.LongClick += BtnSkipPrevious_LongClick;
            btnPlayPause.Click += BtnPlayPause_Click;
            btnSkipNext.Click += BtnSkipNext_Click;
            btnSkipNext.LongClick += BtnSkipNext_LongClick;
            btnLaunchNotification.Click += BtnLaunchNotification_Click;
            skbSeekSongTime.ProgressChanged += SkbSeekSongTime_ProgressChanged;
            skbSeekSongTime.StopTrackingTouch += SkbSeekSongTime_StopTrackingTouch;
            maincontainer.LongClick += MusicPlayerContainer_LongClick;
            maincontainer.Click += MusicPlayerContainer_Click;
            
        }

        private void BtnLaunchNotification_Click(object sender, EventArgs e)
        {
            if (MusicController.MediaActivatedWithThisToken(openNotification.GetMediaSessionToken()))
            {
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs 
                { Active = false, 
                    Show = true,
                    WidgetName = "NotificationFragment", 
                    WidgetAskingForShowing= "MusicFragment", 
                    AdditionalInfo= openNotification});
            }
        }

        private void MusicPlayerContainer_Click(object sender, EventArgs e)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                if (KeyguardHelper.IsDeviceCurrentlyLocked())
                {
                    KeyguardHelper.RequestDismissKeyguard(Activity);
                }

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

                int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, ConfigurationParameters.DefaultAlbumartOpacityLevel);
                int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, ConfigurationParameters.DefaultAlbumartBlurLevel);
                CurrentAlbumArt = new BitmapDrawable(Resources, e.AlbumArt);

                if (configurationManager.RetrieveAValue(ConfigurationParameters.ShowAlbumArt))
                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        Wallpaper = new BitmapDrawable(Resources, e.AlbumArt),
                        OpacityLevel = (short)opacitylevel,
                        BlurLevel = (short) blurLevel, 
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
                tvTitle.Text = e.MediaMetadata?.GetString(MediaMetadata.MetadataKeyTitle);
                tvAlbum.Text = e.MediaMetadata?.GetString(MediaMetadata.MetadataKeyAlbum);
                tvArtist.Text = e.MediaMetadata?.GetString(MediaMetadata.MetadataKeyArtist);
                skbSeekSongTime.Max = (int)e.MediaMetadata?.GetLong(MediaMetadata.MetadataKeyDuration)/1000;
                openNotification = e.OpenNotification;
                if (e.AppName != string.Empty)
                {
                    sourceApp.Text = string.Format(Resources.GetString(Resource.String.playing_from_template), e.AppName);
                }


                ThreadPool.QueueUserWorkItem(m =>
                {
                    var albumart = e.MediaMetadata?.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);
                    var wallpaper = new BitmapDrawable(Resources, albumart);
                    int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, ConfigurationParameters.DefaultAlbumartOpacityLevel);
                    int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, ConfigurationParameters.DefaultAlbumartBlurLevel);
                    CurrentAlbumArt = wallpaper;

                    if (configurationManager.RetrieveAValue(ConfigurationParameters.ShowAlbumArt))
                        WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                        {
                            Wallpaper = wallpaper,
                            OpacityLevel = (short)opacitylevel,
                            BlurLevel = (short) blurLevel,
                            WallpaperPoster = WallpaperPoster.MusicPlayer //We must nutify WallpaperPublisher who is posting the wallpaper, otherwise it'll be ignored.
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
                        //Start timeout to hide the MusicFragment (but only if the music method chosen is 'Pick a MediaSession' (0)                        
                        //Otherwise, the Music Widget can only disappear when the notification is removed. (which is the correct behavior)
                        if (configurationManager.RetrieveAValue(ConfigurationParameters.MusicWidgetMethod, "1") == "0")
                        {
                            StartTimeout(true); 
                        }
                        MoveSeekbar(false);

                        break;

                    case PlaybackStateCode.Playing:
                        btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_pause_white_24dp, 0, 0);
                        playbackState = PlaybackStateCode.Playing;
                        StartTimeout(false);
                        MoveSeekbar(true);

                        break;

                    case PlaybackStateCode.Stopped:
                        HideMusicWidget();
                        btnPlayPause.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, Resource.Drawable.ic_play_arrow_white_24dp, 0, 0);
                        playbackState = PlaybackStateCode.Stopped;
                        MoveSeekbar(false);
                        break;

                    default:
                        break;
                }
                skbSeekSongTime.SetProgress((int)e.CurrentTime/1000, true);
            });
        }

        #endregion Subscribing and Reacting to events

        private void BindViews(View view)
        {
            tvTitle = view.FindViewById<TextView>(Resource.Id.tvSongName);
            tvAlbum = view.FindViewById<TextView>(Resource.Id.tvAlbumName);
            tvArtist = view.FindViewById<TextView>(Resource.Id.tvArtistName);
            sourceApp = view.FindViewById<TextView>(Resource.Id.sourceapp);
            playbackstatus = view.FindViewById<TextView>(Resource.Id.playbackstatus);

            btnSkipPrevious = view.FindViewById<Button>(Resource.Id.btnMediaPrevious);
            btnPlayPause = view.FindViewById<Button>(Resource.Id.btnMediaPlayPlause);
            btnSkipNext = view.FindViewById<Button>(Resource.Id.btnMediaNext);
            btnLaunchNotification = view.FindViewById<Button>(Resource.Id.btnLaunchNotification);

            skbSeekSongTime = view.FindViewById<SeekBar>(Resource.Id.seeksongTime);


            maincontainer = view.FindViewById<LinearLayout>(Resource.Id.container);
        }
        private void UnbindViews()
        {
            tvTitle.Dispose();
            tvAlbum.Dispose();
            tvArtist.Dispose();

            btnSkipPrevious.Dispose();
            btnPlayPause.Dispose();
            btnSkipNext.Dispose();
            skbSeekSongTime.Dispose();

            maincontainer.Dispose();

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
            //Start
            if (move)
            {
                handler.RemoveCallbacks(runnable);
                handler.PostDelayed(runnable, 1000);
            }
            else
            {
                handler.RemoveCallbacks(runnable);
            }
        }

        private void MoveSeekbar()
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.M)
            {
                skbSeekSongTime.SetProgress(skbSeekSongTime.Progress + 1, true);
            }
            else
            {
                skbSeekSongTime.Progress += 1;
            }
            handler.PostDelayed(runnable, 1000);
        }

        private void StartTimeout(bool start)
        {
            if (start==false)
            {
                maincontainer?.RemoveCallbacks(HideMusicWidget); //Stop counting.
                return;
            }
            else
            {
                if (timeoutStarted == true)
                {
                    maincontainer?.RemoveCallbacks(HideMusicWidget);
                    maincontainer?.PostDelayed(HideMusicWidget, 7000);
                }
                //If not, simply wait 7 seconds then hide the notification, in that span of time, the timeout is
                //marked as Started(true)
                else
                {
                    timeoutStarted = true;
                    maincontainer?.PostDelayed(HideMusicWidget, 7000);
                }
            }
        }
        void HideMusicWidget()
        {
            if (maincontainer != null)
            {
                maincontainer.Visibility = ViewStates.Gone;
                timeoutStarted = false;
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "MusicFragment", Active=false });
            }
        }

    }
}