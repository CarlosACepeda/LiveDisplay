using Android.Animation;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using LiveDisplay.Services.Media;
using LiveDisplay.Services.Media.MediaEventArgs;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using LiveDisplay.Services.Wallpaper;
using System;
using System.Linq;
using System.Threading;
using System.Timers;
using Fragment = AndroidX.Fragment.App.Fragment;
using Timer = System.Timers.Timer;

namespace LiveDisplay.Fragments
{
    public class MediaFragment : Fragment
    {
        TextView tvTitle, tvArtist, tvAlbum, sourceApp;
        ImageButton btnSkipPrevious, 
            btnPlayPause, btnSkipNext, discardMediaSession, repeat, toggleAdditionalControls;
        ProgressBar buffering;
        LinearLayout maincontainer, additionalMediaControls;
        TextView noMediaPlaying;
        SeekBar skbSeekSongTime;
        Timer fastForwardTimer;
        Timer rewindTimer;
        bool longPressStarted = false;
        ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
        OpenNotification currentMediaNotification;
        IMediaControls mediaControls;
        float initialX=0;
        float pixelToMoveTo = 0;
        bool isPixelWithinBounds;
        long touchDownTime;

        int lowestBoundary, highestBoundary;
        Timer discardMediaSessionButtonTimeOut;
        bool discardMediaSessionClicked;
        PlaybackStateCode playbackState;
        public override void OnCreate(Bundle savedInstanceState)
        {
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
                mediaControls = MediaControlsKitkat.GetInstance();
            }
            else 
            {
                mediaControls = MediaControlsLollipop.GetInstance();

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
            BindMediaControllerEvents();
            fastForwardTimer.Elapsed += FastForwardTimer_Elapsed;
            rewindTimer.Elapsed += RewindTimer_Elapsed;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                currentMediaNotification = CatcherHelper.FindMostRecentMediaNotification();
                if (currentMediaNotification != null && currentMediaNotification.MediaSessionToken!= null)
                {
                    MediaEventsPublisherLollipop.InitializeFromToken(currentMediaNotification.MediaSessionToken);
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
            fastForwardTimer.Elapsed -= FastForwardTimer_Elapsed;
            rewindTimer.Elapsed -= RewindTimer_Elapsed;
            UnbindMediaControllerEvents();
            base.OnDestroyView();
        }

        public override void OnDestroy()
        {
            Console.WriteLine("FRAGMENT: onDestroy");
            tvAlbum = null;
            tvArtist = null;
            tvTitle = null;
            skbSeekSongTime = null;
            base.OnDestroy();
        }
        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            //In Kitkat, a notification can never be a MediaStyle, that's why we instance the MediaEventsPublisherLollipop directly

            if(e.OpenNotification.Style== OpenNotification.MediaStyle)
            {
                var mediaSessionToken= e.OpenNotification.MediaSessionToken;
                if(e.OpenNotification.IsOngoing || !e.OpenNotification.IsAutoCancellable)
                {
                    if (MediaEventsPublisherLollipop.IsInitialized() && 
                        MediaEventsPublisherLollipop.GetInstance().IsMediaSessionUsingToken(mediaSessionToken))
                        ToggleMediaControlsVisibility(true);
                    else
                    {
                        Console.WriteLine($"Trying initializing Media for: {e.OpenNotification.AppName}");
                        MediaEventsPublisherLollipop.InitializeFromToken(mediaSessionToken);
                    }
                    currentMediaNotification = e.OpenNotification;
                    LoadAdditionalControls(); //Find a better way to update  the additional controls without reloading all of them
                }
            }
        }

        private bool LoadAdditionalControls()
        {
            if(currentMediaNotification == null) return false;
            var compactViewIndices = currentMediaNotification.CompactViewActionsIndices;
            var notificationActions = currentMediaNotification.Actions;
            if (notificationActions.Count == 0) return false;

            int actionPosition = 0;
            for(int i=0; i<notificationActions.Count; i++)
            {
                if(!compactViewIndices.Contains(i))
                {
                    SetAdditionalControl(notificationActions[i], actionPosition++);
                }
            }
            return true;
        }
        void SetAdditionalControl(OpenAction action, int position)
        {
            var imageButton= additionalMediaControls.GetChildAt(position) as AppCompatImageButton;

            imageButton.SetImageDrawable(action.Icon);
            imageButton.Click += (sender, e) =>
            {
                NotificationSlave.GetInstance().ClickAction(action);
            };
        }

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            if (e.OpenNotification.Style == OpenNotification.MediaStyle)
            {
                if (MediaEventsPublisherLollipop.IsInitialized() &&
                    MediaEventsPublisherLollipop.GetInstance().IsMediaSessionUsingToken(e.OpenNotification.MediaSessionToken))
                {
                    if(MediaEventsPublisherLollipop.GetInstance().Finish(e.OpenNotification.MediaSessionToken))
                    {
                        ToggleMediaControlsVisibility(false);
                        currentMediaNotification = null;
                    }
                }
            }
        }

        #region Fragment Views events
        private void BindViewEvents()
        {
            btnSkipPrevious.Click += BtnSkipPrevious_Click;
            btnSkipPrevious.Touch += BtnSkipPrevious_Touch;
            btnSkipPrevious.LongClick += BtnSkipPrevious_LongClick;
            btnPlayPause.Click += BtnPlayPause_Click;
            btnSkipNext.Click += BtnSkipNext_Click;
            btnSkipNext.Touch += BtnSkipNext_Touch;
            btnSkipNext.LongClick += BtnSkipNext_LongClick;
            skbSeekSongTime.StopTrackingTouch += SkbSeekSongTime_StopTrackingTouch;
            maincontainer.LongClick += MusicPlayerContainer_LongClick;
            maincontainer.Click += MusicPlayerContainer_Click;
            maincontainer.Touch += Maincontainer_Touch;
            discardMediaSession.Click += DiscardMediaSession_Click;
            repeat.Click += Repeat_Click;
            toggleAdditionalControls.Click += ToggleAdditionalControls_Click;
        }

        private void ToggleAdditionalControls_Click(object sender, EventArgs e)
        {
            AnimateToggleAdditionalControls(additionalMediaControls.Visibility);
        }
        void AnimateToggleAdditionalControls(ViewStates visibility)
        {
            int oneEightofASecond = 1000/8;
            Rect additionalControlsRect= new Rect();
            additionalMediaControls.GetGlobalVisibleRect(additionalControlsRect);

            var height = additionalControlsRect.Height();

            Activity.RunOnUiThread(() =>
            {
                ValueAnimator animator = visibility== ViewStates.Visible? ValueAnimator.OfFloat(height*-1, 0):
                                    ValueAnimator.OfFloat(0, height*- 1);

                animator.SetInterpolator(new OvershootInterpolator());
                animator.SetDuration(oneEightofASecond);
                animator.Start();
                animator.Update += (sender, e) =>
                {
                    maincontainer.SetY((float)e.Animation.AnimatedValue);
                    if((int)e.Animation.AnimatedValue == height || (int)e.Animation.AnimatedValue== 0)
                    {
                        switch (visibility)
                        {
                            case ViewStates.Visible:
                                additionalMediaControls.Visibility = ViewStates.Invisible;
                                break;
                            default:
                                additionalMediaControls.Visibility = ViewStates.Visible;
                                var loadedControls= LoadAdditionalControls();
                                if(!loadedControls)
                                {
                                    AnimateToggleAdditionalControls(visibility); //it'll cause to hide itself.
                                }
                                break;
                        }
                    }
                    };
                Console.WriteLine("Animating Additional Controls");
            }
            );
        }

        private void Repeat_Click(object sender, EventArgs e)
        {
             mediaControls.CycleRepeatOption();
        }

        private void DiscardMediaSession_Click(object sender, EventArgs e)
        {
            //We can't discard a Media session that's active, let's pause it.
            mediaControls.Pause();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) //In kitkat ther's not a notification attached to the Media playing
            {
                NotificationSlave.GetInstance().CancelNotification(currentMediaNotification?.Key); //Now it should let us remove the notification.
            }
            discardMediaSessionClicked = true; //Set a flag, for when the Media Session changes its state to paused.
        }

        private void Maincontainer_Touch(object sender, View.TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    initialX = e.Event.GetX();
                    touchDownTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                    break;
                case MotionEventActions.Move:
                    {
                        Rect discardMediaSesisonButtonXWidth = new Rect();
                        discardMediaSession.GetDrawingRect(discardMediaSesisonButtonXWidth);


                        lowestBoundary = discardMediaSesisonButtonXWidth.Left;
                        highestBoundary = discardMediaSesisonButtonXWidth.Right;

                        pixelToMoveTo = e.Event.RawX - initialX;

                        isPixelWithinBounds = pixelToMoveTo < highestBoundary && pixelToMoveTo > lowestBoundary;

                        if (pixelToMoveTo > highestBoundary)
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
                            }
                        }
                        else if (pixelToMoveTo < lowestBoundary)
                        {
                            pixelToMoveTo = lowestBoundary;
                        }
                        else if (isPixelWithinBounds)
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

                    int Xdiff = (int)(e.Event.RawX - initialX);
                    if (Java.Lang.JavaSystem.CurrentTimeMillis() - touchDownTime < 100 && (Xdiff < 10))
                    {
                        maincontainer.PerformClick();
                    }

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
            mediaControls.SeekTo(skbSeekSongTime.Progress - 5000); //The timer Elapsed event doesn't fire immmediately, so Ill help it, giving it a kickstart, so to speak.
            rewindTimer.Start();
        }
        private void BtnSkipNext_LongClick(object sender, View.LongClickEventArgs e)
        {
            longPressStarted = true;
            mediaControls.SeekTo(skbSeekSongTime.Progress + 5000); //The timer Elapsed event doesn't fire immmediately, so Ill help it, giving it a kickstart, so to speak.
            fastForwardTimer.Start();
        }

        private void MusicPlayerContainer_Click(object sender, EventArgs e)
        {
            try { mediaControls.OpenRelatedActivity(); }
            catch (PendingIntent.CanceledException ex)
            {   Console.WriteLine($"Failed Sending PendingIntent: {ex.Message}");

                if(currentMediaNotification!= null)
                NotificationSlave.GetInstance().ClickNotification(currentMediaNotification);
            }
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
            mediaControls.SeekTo(skbSeekSongTime.Progress + 5000);
        }
        private void RewindTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mediaControls.SeekTo(skbSeekSongTime.Progress - 5000);
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
            SetSeekbarProgress(e.SeekBar.Progress);
            mediaControls.SeekTo(e.SeekBar.Progress);
        }

        private void SetSeekbarProgress(int progress, int max=0)
        {
            Activity.RunOnUiThread(() =>
            {
                if (max > 0) 
                {
                    skbSeekSongTime.Max = max;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    skbSeekSongTime.SetProgress(progress, true);
                }
                else
                {
                    skbSeekSongTime.Progress = progress;
                }
            });
        }

        void SetRepeatOption(int repeatOption)
        {
            Activity?.RunOnUiThread(() =>
            {
                var newTheme = Resources.NewTheme();
                switch (repeatOption)
                {
                    case IMediaEventsPublisher.DontRepeat:
                        repeat.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.outline_repeat_white_24, newTheme));
                        break;
                    case IMediaEventsPublisher.RepeatOnce:
                        repeat.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.outline_repeat_one_white_24, newTheme));
                        break;
                    case IMediaEventsPublisher.RepeatForever:
                        repeat.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.outline_repeat_on_white_24, newTheme));
                        break;
                    default:
                        repeat.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.outline_repeat_white_24, newTheme));
                        break;
                }
            });
        }

        private void BtnSkipNext_Click(object sender, EventArgs e)
        {
            mediaControls.SkipToNext();
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            switch (playbackState)
            {
                case PlaybackStateCode.Paused:
                    mediaControls.Play();
                    break;
                case PlaybackStateCode.Playing:
                    mediaControls.Pause();
                    break;
                default:
                    break;
            }
        }

        private void BtnSkipPrevious_Click(object sender, EventArgs e)
        {
            mediaControls.SkipToPrevious();
        }

        #endregion Fragment Views events

        private void BindMediaControllerEvents()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                MediaEventsPublisherKitkat.MediaPlaybackChanged += MusicController_MediaPlaybackChanged;
                MediaEventsPublisherKitkat.MediaMetadataChanged += MusicController_MediaMetadataChanged;
                MediaEventsPublisherKitkat.MediaProgressChanged += MediaEventsPublisherKitkat_MediaProgressChanged;
                MediaEventsPublisherKitkat.MediaRepeatOptionChanged += MediaEventsPublisherKitkat_MediaRepeatOptionChanged;
            }
            else
            {
                MediaEventsPublisherLollipop.MediaPlaybackChanged += MusicController_MediaPlaybackChanged;
                MediaEventsPublisherLollipop.MediaMetadataChanged += MusicController_MediaMetadataChanged;
                MediaEventsPublisherLollipop.MediaProgressChanged += MediaEventsPublisherLollipop_MediaProgressChanged;
                MediaEventsPublisherLollipop.MediaRepeatOptionChanged += MediaEventsPublisherLollipop_MediaRepeatOptionChanged;
            }
        }
        private void UnbindMediaControllerEvents()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                MediaEventsPublisherKitkat.MediaPlaybackChanged -= MusicController_MediaPlaybackChanged;
                MediaEventsPublisherKitkat.MediaMetadataChanged -= MusicController_MediaMetadataChanged;
                MediaEventsPublisherKitkat.MediaProgressChanged -= MediaEventsPublisherKitkat_MediaProgressChanged;
                MediaEventsPublisherKitkat.MediaRepeatOptionChanged -= MediaEventsPublisherKitkat_MediaRepeatOptionChanged;
            }
            else
            {
                MediaEventsPublisherLollipop.MediaPlaybackChanged -= MusicController_MediaPlaybackChanged;
                MediaEventsPublisherLollipop.MediaMetadataChanged -= MusicController_MediaMetadataChanged;
                MediaEventsPublisherLollipop.MediaProgressChanged -= MediaEventsPublisherLollipop_MediaProgressChanged;
                MediaEventsPublisherLollipop.MediaRepeatOptionChanged -= MediaEventsPublisherLollipop_MediaRepeatOptionChanged;
            }
        }

        private void MediaEventsPublisherLollipop_MediaRepeatOptionChanged(object sender, int e)
        {
            SetRepeatOption(e);
        }

        private void MediaEventsPublisherLollipop_MediaProgressChanged(object sender, MediaProgressChangedEventArgs e)
        {
            SetSeekbar(e);
        }

        private void MediaEventsPublisherKitkat_MediaRepeatOptionChanged(object sender, int e)
        {
            SetRepeatOption(e);
        }

        private void MediaEventsPublisherKitkat_MediaProgressChanged(object sender, MediaProgressChangedEventArgs e)
        {
            SetSeekbar(e);
        }

        private void SetSeekbar(MediaProgressChangedEventArgs e)
        {
            SetSeekbarProgress((int)e.CurrentProgress, (int)e.TotalProgress);
        }

        private void MusicController_MediaMetadataChanged(object sender, MediaMetadataChangedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                bool isKitkat = Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch;

                tvTitle.Text = isKitkat ? 
                e.MediaMetadataKitkat.GetString((MediaMetadataEditKey)MetadataKey.Title, string.Empty): 
                e.MediaMetadata?.GetString(MediaMetadata.MetadataKeyTitle);

                tvAlbum.Text =  isKitkat ? 
                e.MediaMetadataKitkat.GetString((MediaMetadataEditKey)MetadataKey.Album, string.Empty):
                e.MediaMetadata?.GetString(MediaMetadata.MetadataKeyAlbum);

                tvArtist.Text = isKitkat?
                e.MediaMetadataKitkat.GetString((MediaMetadataEditKey)MetadataKey.Artist, string.Empty)
                : e.MediaMetadata?.GetString(MediaMetadata.MetadataKeyArtist);

                var duration= isKitkat ?
                (int)e.MediaMetadataKitkat.GetLong((MediaMetadataEditKey)MetadataKey.Duration, 0) :
                (int)e.MediaMetadata?.GetLong(MediaMetadata.MetadataKeyDuration); //In ms

                if (duration > 0)
                {
                    skbSeekSongTime.Max = duration;
                    skbSeekSongTime.Enabled = true;
                }
                else
                    skbSeekSongTime.Enabled = false;


                sourceApp.Text = string.Format(Resources.GetString(Resource.String.playing_from_template), e.AppName);
                ThreadPool.QueueUserWorkItem(m =>
                {
                    var albumart = isKitkat?
                    e.MediaMetadataKitkat.GetBitmap(MediaMetadataEditKey.BitmapKeyArtwork, null):
                    e.MediaMetadata?.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);

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

                SetRepeatOption(e.RepeatOptionSet);
                SetAvailableControls(e.SupportedActions);

                switch (e.PlaybackState)
                {
                    case PlaybackStateCode.Paused:
                        btnPlayPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp, Resources.NewTheme()));
                        Console.WriteLine("PLAYBACK PAUSED");
                        break;

                    case PlaybackStateCode.Playing:
                        btnPlayPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp, Resources.NewTheme()));
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
                        break;

                    case PlaybackStateCode.Buffering:
                        buffering.Visibility = ViewStates.Visible;
                        btnPlayPause.Visibility = ViewStates.Gone;
                        break;
                    case PlaybackStateCode.None:
                        //Indicates that the session has no media to play
                        ToggleMediaControlsVisibility(false);
                        break;

                    default:
                        break;
                }

                if (discardMediaSessionClicked &&
                e.PlaybackState != PlaybackStateCode.Playing)
                {
                    //It means this is the result of a clicking on the discard media session button, and we should hide the controls
                    ToggleMediaControlsVisibility(false);
                    discardMediaSessionClicked = false; //reset flag.
                }
            });
        }

        void SetAvailableControls(MediaSessionSupportedActionsFlags supportedActionsFlags)
        {

            Console.WriteLine($"SupportedActions: {supportedActionsFlags} ");

            SetControlAvailability(btnSkipNext, supportedActionsFlags, MediaSessionSupportedActionsFlags.SkipToNext);
            SetControlAvailability(btnSkipPrevious, supportedActionsFlags, MediaSessionSupportedActionsFlags.SkipToPrevious);
            SetControlAvailability(btnPlayPause, supportedActionsFlags,MediaSessionSupportedActionsFlags.PlayPause);
        }


        void SetControlAvailability(View control, MediaSessionSupportedActionsFlags supportedActionFlags, MediaSessionSupportedActionsFlags toCheck)
        {
            SetControlVisibility(control, supportedActionFlags.HasFlag(toCheck));
        }


        void SetControlVisibility(View control, bool visible)
        {
            control.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
        }

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
            repeat= view.FindViewById<ImageButton>(Resource.Id.repeat);

            skbSeekSongTime = view.FindViewById<SeekBar>(Resource.Id.seeksongTime);


            maincontainer = view.FindViewById<LinearLayout>(Resource.Id.container);
            additionalMediaControls = view.FindViewById<LinearLayout>(Resource.Id.additional_media_controls);
            noMediaPlaying = view.FindViewById<TextView>(Resource.Id.no_media_playing);
            discardMediaSession = view.FindViewById<ImageButton>(Resource.Id.discard_media_session);
            toggleAdditionalControls = view.FindViewById<ImageButton>(Resource.Id.toggle_additional_controls);

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