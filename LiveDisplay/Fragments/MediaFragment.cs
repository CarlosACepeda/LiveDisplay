using Android.Animation;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Music.MediaEventArgs;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Threading;
using System.Timers;
using Fragment = AndroidX.Fragment.App.Fragment;
using Timer = System.Timers.Timer;

namespace LiveDisplay.Fragments
{
    public class MediaFragment : Fragment
    {
        TextView tvTitle, tvArtist, tvAlbum, sourceApp;
        ImageButton btnSkipPrevious, btnPlayPause, btnSkipNext, discardMediaSession;
        ProgressBar buffering;
        LinearLayout maincontainer;
        TextView noMediaPlaying;
        SeekBar skbSeekSongTime;
        PendingIntent activityIntent; //A Pending intent if available to start the activity associated with this music fragent.
        Timer timer;
        Timer fastForwardTimer;
        Timer rewindTimer;
        bool longPressStarted = false;
        ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
        OpenNotification currentMediaNotification;
        IMusicControls musicControls;
        float initialX=0;
        float pixelToMoveTo = 0;
        bool isPixelWithinBounds;
        int lowestBoundary, highestBoundary;
        Timer discardMediaSessionButtonTimeOut;
        bool discardMediaSessionClicked;
        PlaybackStateCode playbackState;
        RemoteControlPlayState playbackStateKitkat;

        public override void OnCreate(Bundle savedInstanceState)
        {

            timer = new Timer
            {
                Interval = 1000 //1 second.
            };
            fastForwardTimer = new Timer
            {
                Interval = 1000
            };
            rewindTimer = new Timer
            {
                Interval = 1000
            };
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                musicControls = MusicControlsKitkat.GetInstance();
            }
            else 
            {
                musicControls = MusicControlsLollipop.GetInstance();
                
            }

            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;

            Console.WriteLine("FRAGMENT: onCreate");
            base.OnCreate(savedInstanceState);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Media, container, false);
            Console.WriteLine("FRAGMENT: onCreateView");
            BindViews(view);
            BindViewEvents();
            BindMusicControllerEvents();
            timer.Elapsed += Timer_Elapsed;
            fastForwardTimer.Elapsed += FastForwardTimer_Elapsed;
            rewindTimer.Elapsed += RewindTimer_Elapsed;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                currentMediaNotification = CatcherHelper.FindMostRecentMediaNotification();
                if (currentMediaNotification != null)
                {
                    MediaEventsPublisherLollipop.InitializeFromToken(currentMediaNotification.GetMediaSessionToken());
                }
            }

            return view;
        }
        public override void OnResume()
        {
            Console.WriteLine("FRAGMENT: onResume");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                if (MediaEventsPublisherLollipop.IsInitialized() || currentMediaNotification != null)
                {
                    Console.WriteLine("CONTROLS NOT VISIBLE, MAKING'EM VISIBLE");
                    ToggleMediaControlsVisibility(true);
                }
                else ToggleMediaControlsVisibility(false);

            }
            else
            {
                if(MediaEventsPublisherKitkat.IsInitialized())
                {
                    Console.WriteLine("CONTROLS NOT VISIBLE, MAKING'EM VISIBLE");
                    ToggleMediaControlsVisibility(true);
                }
                else ToggleMediaControlsVisibility(false);

            }
          
            base.OnResume();
        }
        public override void OnDestroyView()
        {
            Console.WriteLine("FRAGMENT: onDestroView");
            timer.Elapsed -= Timer_Elapsed;
            fastForwardTimer.Elapsed -= FastForwardTimer_Elapsed;
            rewindTimer.Elapsed -= RewindTimer_Elapsed;
            UnbindViewEvents();

            base.OnDestroyView();
        }

        public override void OnDestroy()
        {
            Console.WriteLine("FRAGMENT: onDestroy");
            tvAlbum = null;
            tvArtist = null;
            tvTitle = null;
            skbSeekSongTime = null;
            timer.Dispose();
            base.OnDestroy();
        }



        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            //In Kitkat, a notification can never be a MediaStyle, that's why we instance the MediaEventsPublisherLollipop directly
            if(e.OpenNotification.Style()== OpenNotification.MediaStyle && e.OpenNotification.IsOnGoing())
            {
                var mediaSessionToken = e.OpenNotification.GetMediaSessionToken();
                    //Only try to initialize if it isn't initialized already
                    //or if the Current existing MediaSession is not active and is different than the one incoming
                    if(!MediaEventsPublisherLollipop.IsInitialized() 
                        || 
                        (!MediaEventsPublisherLollipop.GetInstance().IsMediaSessionUsingToken(mediaSessionToken)
                        ))
                    {
                        Console.WriteLine($"Trying initializing Media for: {e.OpenNotification.AppName()}");
                        MediaEventsPublisherLollipop.InitializeFromToken(mediaSessionToken);
                        currentMediaNotification = e.OpenNotification;
                    }
                    ToggleMediaControlsVisibility(true);
            }
        }

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            if (e.OpenNotification.Style() == OpenNotification.MediaStyle)
            {
                if (MediaEventsPublisherLollipop.IsInitialized() &&
                    MediaEventsPublisherLollipop.GetInstance().IsMediaSessionUsingToken(e.OpenNotification.GetMediaSessionToken()))
                {
                    if(MediaEventsPublisherLollipop.GetInstance().Finish(e.OpenNotification.GetMediaSessionToken()))
                    {
                        ToggleMediaControlsVisibility(false);
                        currentMediaNotification = null;
                    }
                }
            }
        }

        #region Fragment Views events

        private void UnbindMusicControllerEvents()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                MediaEventsPublisherKitkat.MediaPlaybackChanged -= MusicControllerKitkat_MediaPlaybackChanged;
                MediaEventsPublisherKitkat.MediaMetadataChanged -= MusicControllerKitkat_MediaMetadataChanged;
            }
            else
            {
                MediaEventsPublisherLollipop.MediaPlaybackChanged -= MusicController_MediaPlaybackChanged;
                MediaEventsPublisherLollipop.MediaMetadataChanged -= MusicController_MediaMetadataChanged;
            }
        }


        private void UnbindViewEvents()
        {
            btnSkipPrevious.Click -= BtnSkipPrevious_Click;
            btnSkipPrevious.Touch -= BtnSkipPrevious_Touch;
            btnSkipPrevious.LongClick -= BtnSkipPrevious_LongClick;
            btnPlayPause.Click -= BtnPlayPause_Click;
            btnSkipNext.Click -= BtnSkipNext_Click;
            btnSkipNext.Touch -= BtnSkipNext_Touch;
            btnSkipNext.LongClick -= BtnSkipNext_LongClick;
            skbSeekSongTime.ProgressChanged -= SkbSeekSongTime_ProgressChanged;
            skbSeekSongTime.StopTrackingTouch -= SkbSeekSongTime_StopTrackingTouch;
            maincontainer.LongClick -= MusicPlayerContainer_LongClick;
            maincontainer.Click -= MusicPlayerContainer_Click;
            discardMediaSession.Click -= DiscardMediaSession_Click;

        }
        private void BindViewEvents()
        {
            btnSkipPrevious.Click += BtnSkipPrevious_Click;
            btnSkipPrevious.Touch += BtnSkipPrevious_Touch;
            btnSkipPrevious.LongClick += BtnSkipPrevious_LongClick;
            btnPlayPause.Click += BtnPlayPause_Click;
            btnSkipNext.Click += BtnSkipNext_Click;
            btnSkipNext.Touch += BtnSkipNext_Touch;
            btnSkipNext.LongClick += BtnSkipNext_LongClick;
            skbSeekSongTime.ProgressChanged += SkbSeekSongTime_ProgressChanged;
            skbSeekSongTime.StopTrackingTouch += SkbSeekSongTime_StopTrackingTouch;
            maincontainer.LongClick += MusicPlayerContainer_LongClick;
            maincontainer.Click += MusicPlayerContainer_Click;
            maincontainer.Touch += Maincontainer_Touch;
            discardMediaSession.Click += DiscardMediaSession_Click;
        }

        private void DiscardMediaSession_Click(object sender, EventArgs e)
        {
            //We can't discard a Media session that's active, let's pause it.
            musicControls.Pause();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) //In kitkat ther's not a notification attached to the Media playing
            {
                NotificationSlave.NotificationSlaveInstance().CancelNotification(currentMediaNotification?.GetKey()); //Now it should let us remove the notification.
            }
            discardMediaSessionClicked = true; //Set a flag, for when the Media Session changes its state to paused.
        }

        private void Maincontainer_Touch(object sender, View.TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    initialX = e.Event.GetX();
                    break;
                case MotionEventActions.Move:
                    {
                        Rect discardMediaSesisonButtonXWidth = new Rect();
                        discardMediaSession.GetDrawingRect(discardMediaSesisonButtonXWidth);

                        
                        lowestBoundary = discardMediaSesisonButtonXWidth.Left;
                        highestBoundary = discardMediaSesisonButtonXWidth.Right;

                        pixelToMoveTo =  e.Event.RawX - initialX;

                        isPixelWithinBounds = pixelToMoveTo < highestBoundary && pixelToMoveTo > lowestBoundary;

                        if(pixelToMoveTo> highestBoundary)
                        {
                            pixelToMoveTo = highestBoundary;
                            if (discardMediaSessionButtonTimeOut == null || !discardMediaSessionButtonTimeOut.Enabled)
                            {
                                Console.WriteLine("Started timeout for Hiding the MediaDiscard Button");

                                discardMediaSessionButtonTimeOut = new Timer
                                {
                                    Interval = 3000,
                                    AutoReset = false
                                };
                                discardMediaSessionButtonTimeOut.Start();
                                discardMediaSessionButtonTimeOut.Elapsed += (sender, e) =>
                                {
                                    HideMediaDiscardButton(highestBoundary, new AnticipateOvershootInterpolator(), 1000);
                                };
                                //NotificationSlave.NotificationSlaveInstance().CancelNotification(currentNotif.GetUnderlyingStatusBarNotification().Key);
                            }
                        }
                        else if( pixelToMoveTo< lowestBoundary)
                        {
                            pixelToMoveTo = lowestBoundary;
                        }
                        else if(isPixelWithinBounds)
                        {
                            if (discardMediaSessionButtonTimeOut != null && discardMediaSessionButtonTimeOut.Enabled)
                            {
                                discardMediaSessionButtonTimeOut.Stop();
                                Console.WriteLine("Cancelling discard button time out cuz its already moving");
                            }
                        }

                        maincontainer.SetX(pixelToMoveTo);
                        discardMediaSession.Alpha = pixelToMoveTo / highestBoundary;
                    }
                    break;
                case MotionEventActions.Up:
                    if (isPixelWithinBounds)
                    {
                        Console.WriteLine("UP, Pixel within bounds");
                        HideMediaDiscardButton(pixelToMoveTo, new AccelerateInterpolator(), 250);
                    }
                    break;
            }
        }

        private void BtnSkipPrevious_LongClick(object sender, View.LongClickEventArgs e)
        {
            longPressStarted = true;
            musicControls.SeekTo(skbSeekSongTime.Progress - 5000); //The timer Elapsed event doesn't fire immmediately, so Ill help it, giving it a kickstart, so to speak.
            rewindTimer.Start();
        }
        private void BtnSkipNext_LongClick(object sender, View.LongClickEventArgs e)
        {
            longPressStarted = true;
            musicControls.SeekTo(skbSeekSongTime.Progress + 5000); //The timer Elapsed event doesn't fire immmediately, so Ill help it, giving it a kickstart, so to speak.
            fastForwardTimer.Start();
        }

        private void MusicPlayerContainer_Click(object sender, EventArgs e)
        {
            try { activityIntent.Send(); }
            catch { Log.Info("LiveDisplay", "Failed to send the Music pending intent"); }
        }

        private void BtnSkipPrevious_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Up && longPressStarted)
            {
                rewindTimer.Stop();
                longPressStarted = false;
            }
            e.Handled = false; //So click and longclick work.

        }
        private void BtnSkipNext_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Up && longPressStarted)
            {
                fastForwardTimer.Stop();
                longPressStarted = false;
            }
            e.Handled = false; //So click and longclick work.
        }

        private void FastForwardTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            musicControls.SeekTo(skbSeekSongTime.Progress + 5000);
        }
        private void RewindTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            musicControls.SeekTo(skbSeekSongTime.Progress - 5000);
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
            //When user stops dragging then seek to the position previously saved in ProgressChangedEvent
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                skbSeekSongTime.SetProgress(e.SeekBar.Progress, true);
            }
            else
            {
                skbSeekSongTime.Progress = e.SeekBar.Progress;
            }
            musicControls.SeekTo(e.SeekBar.Progress);
        }

        private void SkbSeekSongTime_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            //This will save the current song time.
           
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
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
            musicControls.SkipToNext();
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            if(Build.VERSION.SdkInt>= BuildVersionCodes.Lollipop)
            {
                switch (playbackState)
                {
                    //If the media is paused, then Play.
                    case PlaybackStateCode.Paused:
                        musicControls.Play();
                        break;
                    //If the media is playing, then pause.
                    case PlaybackStateCode.Playing:
                        musicControls.Pause();
                        break;
                    //add more cases and handle them.
                    default:
                        break;
                }
            }
            else
            {
                switch(playbackStateKitkat)
                {
                    case RemoteControlPlayState.Paused:
                        musicControls.Play();
                            break;
                    case RemoteControlPlayState.Playing:
                        musicControls.Pause();
                        break;
                    default:
                        musicControls.Play();
                        break;
                }
            }
            
        }

        private void BtnSkipPrevious_Click(object sender, EventArgs e)
        {
            musicControls.SkipToPrevious();
        }

        #endregion Fragment Views events

        #region Subscribing and Reacting to events

        private void BindMusicControllerEvents()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                MediaEventsPublisherKitkat.MediaPlaybackChanged += MusicControllerKitkat_MediaPlaybackChanged;
                MediaEventsPublisherKitkat.MediaMetadataChanged += MusicControllerKitkat_MediaMetadataChanged;
            }
            else
            {
                MediaEventsPublisherLollipop.MediaPlaybackChanged += MusicController_MediaPlaybackChanged;
                MediaEventsPublisherLollipop.MediaMetadataChanged += MusicController_MediaMetadataChanged;
            }
        }

        private void MusicControllerKitkat_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedEventArgs e)
        {
            Console.WriteLine("MEDIA PLAYBACK CHANGED, FRAGMENT");
            playbackStateKitkat = e.PlaybackStateKitkat;
            Activity?.RunOnUiThread(() =>
            {
                switch (e.PlaybackStateKitkat)
                {
                    case RemoteControlPlayState.Paused:
                        btnPlayPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp, Resources.NewTheme()));

                        MoveSeekbar(false);
                        break;

                    case RemoteControlPlayState.Playing:
                        btnPlayPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp, Resources.NewTheme()));

                        MoveSeekbar(true);

                        break;

                    case RemoteControlPlayState.Stopped:
                        btnPlayPause.Background =
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp, Resources.NewTheme());

                        MoveSeekbar(false);
                        break;

                    default:
                        break;
                }
                if (discardMediaSessionClicked &&
               e.PlaybackStateKitkat != RemoteControlPlayState.Playing)
                {
                    //It means this is the result of a clicking on the discard media session button, and we should hide the controls
                    ToggleMediaControlsVisibility(false);
                    discardMediaSessionClicked = false; //reset flag.
                }
            });
        }

        private void MusicControllerKitkat_MediaMetadataChanged(object sender, MediaMetadataChangedKitkatEventArgs e)
        {
            Console.WriteLine("MEDIA METADATA CHANGED, FRAGMENT");
            Activity?.RunOnUiThread(() =>
            {
                tvTitle.Text = e.Title;
                tvAlbum.Text = e.Album;
                tvArtist.Text = e.Artist;
                skbSeekSongTime.Max = (int)e.Duration;

                int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, ConfigurationParameters.DefaultAlbumartOpacityLevel);
                int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, ConfigurationParameters.DefaultAlbumartBlurLevel);                

                if (configurationManager.RetrieveAValue(ConfigurationParameters.ShowAlbumArt))
                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        Wallpaper = new BitmapDrawable(Resources, e.AlbumArt),
                        OpacityLevel = (short)opacitylevel,
                        BlurLevel = (short) blurLevel, 
                        WallpaperPoster = WallpaperPoster.MusicPlayer //We must nutify WallpaperPublisher who is posting the wallpaper, otherwise it'll be ignored.
                    });
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
                skbSeekSongTime.Max = (int)e.MediaMetadata?.GetLong(MediaMetadata.MetadataKeyDuration); //In ms
                sourceApp.Text = string.Format(Resources.GetString(Resource.String.playing_from_template), e.AppName);
                ThreadPool.QueueUserWorkItem(m =>
                {
                    var albumart = e.MediaMetadata?.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);
                    var wallpaper = new BitmapDrawable(Activity.Resources, albumart);
                    int opacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, ConfigurationParameters.DefaultAlbumartOpacityLevel);
                    int blurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, ConfigurationParameters.DefaultAlbumartBlurLevel);

                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        Wallpaper = wallpaper,
                        OpacityLevel = (short)opacitylevel,
                        BlurLevel = (short)blurLevel,
                        WallpaperPoster = WallpaperPoster.MusicPlayer,
                        SecondsOfAttention = (skbSeekSongTime.Max / 1000) - (skbSeekSongTime.Progress / 1000)
                    });

                });
            });
        }

        private void MusicController_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                playbackState = e.PlaybackState;


                switch (e.PlaybackState)
                {
                    case PlaybackStateCode.Paused:
                        btnPlayPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp, Resources.NewTheme()));
                        
                        //Start timeout to hide the MusicFragment (but only if the music method chosen is 'Pick a MediaSession' (0)
                        //Otherwise, the Music Widget can only disappear when the notification is removed. (which is the correct behavior)
                        if (configurationManager.RetrieveAValue(ConfigurationParameters.MusicWidgetMethod, "1") == "0")
                        {
                            //StartTimeout(true);
                        }
                        MoveSeekbar(false);
                        Console.WriteLine("PLAYBACK PAUSED");
                        break;

                    case PlaybackStateCode.Playing:
                        btnPlayPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp, Resources.NewTheme()));
                        //StartTimeout(false);
                        MoveSeekbar(true);
                        ToggleMediaControlsVisibility(true);
                        buffering.Visibility = ViewStates.Gone;
                        btnPlayPause.Visibility = ViewStates.Visible;
                        Console.WriteLine("PLAYBACK PLAYING");

                        break;

                    case PlaybackStateCode.Stopped:
                        btnPlayPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp, Resources.NewTheme()));
                        Console.WriteLine("STOPPED!!");
                        MoveSeekbar(false);
                        break;

                    case PlaybackStateCode.Buffering:
                        buffering.Visibility = ViewStates.Visible;
                        btnPlayPause.Visibility = ViewStates.Gone;
                        break;
                    case PlaybackStateCode.None:
                        Console.WriteLine("NONE HAPPENED");
                        break;

                    default:
                        break;
                }
                skbSeekSongTime.SetProgress((int)e.CurrentTime, true);
                
                if (discardMediaSessionClicked &&
                e.PlaybackState != PlaybackStateCode.Playing)
                {
                    //It means this is the result of a clicking on the discard media session button, and we should hide the controls
                    ToggleMediaControlsVisibility(false);
                    discardMediaSessionClicked = false; //reset flag.
                }
            });
        }

        #endregion Subscribing and Reacting to events

        private void BindViews(View view)
        {
            tvTitle = view.FindViewById<TextView>(Resource.Id.tvSongName);
            tvAlbum = view.FindViewById<TextView>(Resource.Id.tvAlbumName);
            tvArtist = view.FindViewById<TextView>(Resource.Id.tvArtistName);
            sourceApp = view.FindViewById<TextView>(Resource.Id.sourceapp);

            btnSkipPrevious = view.FindViewById<ImageButton>(Resource.Id.btnMediaPrevious);
            btnPlayPause = view.FindViewById<ImageButton>(Resource.Id.btnMediaPlayPlause);
            btnSkipNext = view.FindViewById<ImageButton>(Resource.Id.btnMediaNext);
            buffering= view.FindViewById<ProgressBar>(Resource.Id.buffering);

            skbSeekSongTime = view.FindViewById<SeekBar>(Resource.Id.seeksongTime);


            maincontainer = view.FindViewById<LinearLayout>(Resource.Id.container);
            noMediaPlaying = view.FindViewById<TextView>(Resource.Id.no_media_playing);
            discardMediaSession = view.FindViewById<ImageButton>(Resource.Id.discard_media_session);

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
            noMediaPlaying.Dispose();

        }

        private void MoveSeekbar(bool move)
        {
            if (move)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void HideMediaDiscardButton(float positionWhereToStartAnim, ITimeInterpolator timeInterpolator, int durationInMillis)
        {
            Activity.RunOnUiThread(() =>
            {
                ValueAnimator animator = ValueAnimator.OfFloat(positionWhereToStartAnim, 0);
                animator.SetInterpolator(timeInterpolator);
                animator.SetDuration(durationInMillis);
                animator.Start();
                animator.Update += (sender, e) =>
                {
                    maincontainer.SetX((float)e.Animation.AnimatedValue);
                    discardMediaSession.Alpha = (float)e.Animation.AnimatedValue / highestBoundary;
                };
                Console.WriteLine("Hidden Media Discard button using animation");
            }
            );
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                skbSeekSongTime.SetProgress(skbSeekSongTime.Progress + 1000, true);
            }
            else
            {
                skbSeekSongTime.Progress += 1000;
            }
        }

        void ToggleMediaControlsVisibility(bool mediaPlaying)
        {
            if (mediaPlaying)
            {
                maincontainer.Visibility = ViewStates.Visible;
                noMediaPlaying.Visibility = ViewStates.Invisible;
            }
            else
            {
                maincontainer.Visibility = ViewStates.Invisible;
                noMediaPlaying.Visibility = ViewStates.Visible;
            }
        }

    }
}