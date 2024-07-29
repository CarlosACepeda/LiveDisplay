using Android.Animation;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.Core.Content.Resources;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using LiveDisplay.Services.Media;
using LiveDisplay.Services.Media.Enums;
using LiveDisplay.Services.Media.MediaEventArgs;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Wallpaper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Fragment = AndroidX.Fragment.App.Fragment;
using Timer = System.Timers.Timer;

namespace LiveDisplay.Fragments
{
    public class MediaFragment : Fragment
    {
        TextView title, artist, album, sourceApp;
        ImageButton skipToPrevious,
            playPause, skipToNext, discardMediaSession, repeat, toggleAdditionalControls, stop,
            customAction1, customAction2;
        ProgressBar buffering;
        LinearLayout maincontainer, additionalMediaControls;
        TextView noMediaPlaying;
        SeekBar skbSeekSongTime;
        Timer fastForwardTimer;
        Timer rewindTimer;
        bool longPressStarted = false;
        ConfigurationManager configurationManager = new ConfigurationManager();
        
        MediaControlsBase mediaControls;
        float initialX=0;
        float pixelToMoveTo = 0;
        bool isPixelWithinBounds;
        long touchDownTime;

        int lowestBoundary, highestBoundary;
        Timer discardMediaSessionButtonTimeOut;
        PlaybackStateCode playbackState;
        List<PlaybackState.CustomAction> mediaSessionCustomActions;

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
            mediaControls = MediaControlsBase.Instance;
            Console.WriteLine("FRAGMENT: onCreate");
            base.OnCreate(savedInstanceState);
        }

        private void MediaEventsPublisherLollipop_PublisherFinished(object sender, bool wasFinished)
        {
            if (wasFinished)
                ToggleMediaControlsVisibility(false);
            else
                Console.WriteLine("MediaEventsPublisher wasn't finished correctly, what can we do?");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Media, container, false);
            Console.WriteLine("FRAGMENT: onCreateView");
            BindViews(view);
            BindViewEvents();
            fastForwardTimer.Elapsed += FastForwardTimer_Elapsed;
            rewindTimer.Elapsed += RewindTimer_Elapsed;
            BindMediaControllerEvents();
            return view;
        }
        public override void OnStart()
        {
            Console.WriteLine("FRAGMENT: onStart!");
            //Here we make sure that the Views are actually loaded and ready to use.
             //it posts updates that I need to load as soon as I start listening to these events.
            base.OnStart();
        }
        public override void OnResume()
        {
            Console.WriteLine("FRAGMENT: onResume");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                if (MediaEventsPublisherLollipop.IsInitialized())
                {
                    Console.WriteLine("CONTROLS NOT VISIBLE, MAKING'EM VISIBLE");
                    ToggleMediaControlsVisibility(true);
                }
                else
                { 
                    ToggleMediaControlsVisibility(false); }

            }
            else
            {
                if(MediaEventsPublisherKitkat.IsInitialized())
                {
                    Console.WriteLine("CONTROLS NOT VISIBLE, MAKING'EM VISIBLE");
                    ToggleMediaControlsVisibility(true);
                }
                {
                    ToggleMediaControlsVisibility(false);
                }

            }
          
            base.OnResume();
        }
        public override void OnStop()
        {
            Console.WriteLine("FRAGMENT: onStop");

            base.OnStop();
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
            base.OnDestroy();
        }

        #region Fragment Views events
        private void BindViewEvents()
        {
            skipToPrevious.Click += BtnSkipPrevious_Click;
            skipToPrevious.Touch += BtnSkipPrevious_Touch;
            skipToPrevious.LongClick += BtnSkipPrevious_LongClick;
            playPause.Click += BtnPlayPause_Click;
            skipToNext.Click += BtnSkipNext_Click;
            skipToNext.Touch += BtnSkipNext_Touch;
            skipToNext.LongClick += BtnSkipNext_LongClick;
            skbSeekSongTime.StopTrackingTouch += SkbSeekSongTime_StopTrackingTouch;
            maincontainer.LongClick += MusicPlayerContainer_LongClick;
            maincontainer.Click += MusicPlayerContainer_Click;
            maincontainer.Touch += Maincontainer_Touch;
            discardMediaSession.Click += DiscardMediaSession_Click;
            repeat.Click += Repeat_Click;
            stop.Click += Stop_Click;
            customAction1.Click += CustomAction_Click;
            customAction2.Click += CustomAction_Click;
            toggleAdditionalControls.Click += ToggleAdditionalControls_Click;
        }

        private void CustomAction_Click(object sender, EventArgs e)
        {
            var customActionView = (ImageButton)sender;

            if (customActionView.Tag is PlaybackState.CustomAction customAction)
                mediaControls.SendCustomAction(customAction);
            else if (customActionView.Tag is OpenAction openAction)
                mediaControls.SendCompactedAction(openAction);

        }

        private void Stop_Click(object sender, EventArgs e)
        {
            mediaControls.Stop();
        }

        private void ToggleAdditionalControls_Click(object sender, EventArgs e)
        {
            AnimateToggleAdditionalControls(additionalMediaControls.Visibility);
        }
        void AnimateToggleAdditionalControls(ViewStates visibility)
        {
            bool noLoadedControls = customAction1.Tag == null && customAction2.Tag == null;
            if (noLoadedControls) return;

            int oneEightofASecond = 1000 / 8;
            Rect additionalControlsRect = new Rect();
            additionalMediaControls.GetGlobalVisibleRect(additionalControlsRect);

            var height = additionalControlsRect.Height();

            Activity.RunOnUiThread(() =>
            {
                ValueAnimator animator = visibility == ViewStates.Visible ? ValueAnimator.OfFloat(height * -1, 0) :
                                    ValueAnimator.OfFloat(0, height * -1);

                animator.SetInterpolator(new OvershootInterpolator());
                animator.SetDuration(oneEightofASecond);
                animator.Start();
                animator.Update += (sender, e) =>
                {
                    maincontainer.SetY((float)e.Animation.AnimatedValue);
                    if ((int)e.Animation.AnimatedValue == height || (int)e.Animation.AnimatedValue == 0)
                    {
                        switch (visibility)
                        {
                            case ViewStates.Visible:
                                additionalMediaControls.Visibility = ViewStates.Invisible;
                                break;
                            default:
                                additionalMediaControls.Visibility = ViewStates.Visible;
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
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && MediaEventsPublisherLollipop.IsInitialized()) 
            {
                MediaEventsPublisherLollipop.GetInstance().Finish();
            }
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
            {   
                Console.WriteLine($"Failed Sending PendingIntent: {ex.Message}");
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
            //SkipToNext is susceptible to having a custom action
            var customAction = GetMainMediaControlCustomAction((ImageButton)sender);
            if (customAction != null)
            {
                mediaControls.SendCustomAction(customAction);
                return;
            }
            mediaControls.SkipToNext();
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            switch (playbackState)
            {
                case PlaybackStateCode.Stopped:
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
            //SkipToPrevious is susceptible to having a custom action
            var customAction = GetMainMediaControlCustomAction((ImageButton)sender);
            if (customAction != null)
            {
                mediaControls.SendCustomAction(customAction);
                return;
            }
            mediaControls.SkipToPrevious();
        }

        PlaybackState.CustomAction GetMainMediaControlCustomAction(ImageButton mainMediaControl)
        {
            var skipNextButton = mainMediaControl;
            var skipNextButtonCustomActionTag = skipNextButton.Tag;
            if(skipNextButtonCustomActionTag!= null)
                return (PlaybackState.CustomAction)skipNextButtonCustomActionTag;

            return null;
        }

        #endregion Fragment Views events

        private void BindMediaControllerEvents()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                MediaEventsPublisherKitkat.MediaPlaybackChanged += MediaController_MediaPlaybackChanged;
                MediaEventsPublisherKitkat.MediaMetadataChanged += MediaController_MediaMetadataChanged;
                MediaEventsPublisherKitkat.MediaProgressChanged += MediaEventsPublisherKitkat_MediaProgressChanged;
                MediaEventsPublisherKitkat.MediaRepeatOptionChanged += MediaEventsPublisherKitkat_MediaRepeatOptionChanged;
                MediaEventsPublisherKitkat.ControlsAvailabilityChanged += MediaEventsPublisherLollipop_OnControlsAvailabilityChanged;

            }
            else
            {
                MediaEventsPublisherLollipop.MediaPlaybackChanged += MediaController_MediaPlaybackChanged;
                MediaEventsPublisherLollipop.MediaMetadataChanged += MediaController_MediaMetadataChanged;
                MediaEventsPublisherLollipop.MediaProgressChanged += MediaEventsPublisherLollipop_MediaProgressChanged;
                MediaEventsPublisherLollipop.MediaRepeatOptionChanged += MediaEventsPublisherLollipop_MediaRepeatOptionChanged;
                MediaEventsPublisherLollipop.PublisherFinished += MediaEventsPublisherLollipop_PublisherFinished;
                MediaEventsPublisherLollipop.ControlsAvailabilityChanged += MediaEventsPublisherLollipop_OnControlsAvailabilityChanged;

            }

        }

        private void MediaEventsPublisherLollipop_OnControlsAvailabilityChanged(object sender, ControlsAvailabilityChangedEventArgs e)
        {
            mediaSessionCustomActions = e.CustomActions;
            Console.WriteLine($"{mediaSessionCustomActions.Count}");
            Console.WriteLine($"{e.CustomActions.Count}");
            Console.WriteLine(e.AvailableControls);


            SetImageButtonEnabledStatus(e.AvailableControls.HasFlag(AvailableControls.PlayPause), playPause);
            buffering.Enabled = e.AvailableControls.HasFlag(AvailableControls.Buffering);
            SetImageButtonEnabledStatus(e.AvailableControls.HasFlag(AvailableControls.SkipToNext), skipToNext);
            SetImageButtonEnabledStatus(e.AvailableControls.HasFlag(AvailableControls.SkipToPrevious), skipToPrevious);
            SetImageButtonEnabledStatus(e.AvailableControls.HasFlag(AvailableControls.Stop), stop);
            SetImageButtonEnabledStatus(e.AvailableControls.HasFlag(AvailableControls.Repeat), repeat);

            if(e.TakeCustomActionsFromNotification)
            {
                FillWithCompactedActions(e.OpenNotification);
            }
            else
            {
                FillWithCustomAction(customAction1, e.OpenNotification);
                FillWithCustomAction(customAction2, e.OpenNotification);
            }
        }

        void SetImageButtonEnabledStatus(bool enabled, ImageButton button)
        {
            button.Enabled = enabled;
            button.Alpha = enabled ? ResourcesCompat.GetFloat(Resources, Resource.Dimension.alpha_totally_visible) 
                : ResourcesCompat.GetFloat(Resources, Resource.Dimension.alpha_half_visibility);
        }
        private void UnbindMediaControllerEvents()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                MediaEventsPublisherKitkat.MediaPlaybackChanged -= MediaController_MediaPlaybackChanged;
                MediaEventsPublisherKitkat.MediaMetadataChanged -= MediaController_MediaMetadataChanged;
                MediaEventsPublisherKitkat.MediaProgressChanged -= MediaEventsPublisherKitkat_MediaProgressChanged;
                MediaEventsPublisherKitkat.MediaRepeatOptionChanged -= MediaEventsPublisherKitkat_MediaRepeatOptionChanged;
                MediaEventsPublisherLollipop.ControlsAvailabilityChanged -= MediaEventsPublisherLollipop_OnControlsAvailabilityChanged;
            }
            else
            {
                MediaEventsPublisherLollipop.MediaPlaybackChanged -= MediaController_MediaPlaybackChanged;
                MediaEventsPublisherLollipop.MediaMetadataChanged -= MediaController_MediaMetadataChanged;
                MediaEventsPublisherLollipop.MediaProgressChanged -= MediaEventsPublisherLollipop_MediaProgressChanged;
                MediaEventsPublisherLollipop.MediaRepeatOptionChanged -= MediaEventsPublisherLollipop_MediaRepeatOptionChanged;
                MediaEventsPublisherLollipop.PublisherFinished -= MediaEventsPublisherLollipop_PublisherFinished;
                MediaEventsPublisherLollipop.ControlsAvailabilityChanged -= MediaEventsPublisherLollipop_OnControlsAvailabilityChanged;
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

        private void MediaController_MediaMetadataChanged(object sender, MediaMetadataChangedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                title.Text = e.MediaTitle;
                album.Text = e.MediaAlbum;
                artist.Text = e.MediaArtist;
                if (e.MediaDuration > 0)
                {
                    skbSeekSongTime.Max = (int)e.MediaDuration;
                    skbSeekSongTime.Enabled = true;
                }
                else
                    skbSeekSongTime.Enabled = false;


                sourceApp.Text = string.Format(Resources.GetString(Resource.String.playing_from_template), e.AppName);
                ThreadPool.QueueUserWorkItem(m =>
                {
                    var wallpaper = new BitmapDrawable(Activity.Resources, e.MediaArtwork);
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

        private void MediaController_MediaPlaybackChanged(object sender, MediaPlaybackStateChangedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                playbackState = e.PlaybackState;

                SetRepeatOption(e.RepeatOptionSet);
                switch (e.PlaybackState)
                {
                    case PlaybackStateCode.Paused:
                        playPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp, Resources.NewTheme()));
                        Console.WriteLine("PLAYBACK PAUSED");
                        break;


                    case PlaybackStateCode.Playing:
                        playPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_pause_white_24dp, Resources.NewTheme()));
                        ToggleMediaControlsVisibility(true);
                        buffering.Visibility = ViewStates.Gone;
                        playPause.Visibility = ViewStates.Visible;
                        Console.WriteLine("PLAYBACK PLAYING");

                        break;

                    case PlaybackStateCode.Stopped:
                        playPause.SetImageDrawable(
                        Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1 ?
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp) :
                        Resources.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp, Resources.NewTheme()));
                        Console.WriteLine("STOPPED!!");
                        break;

                    case PlaybackStateCode.Buffering:
                        buffering.Visibility = ViewStates.Visible;
                        playPause.Visibility = ViewStates.Gone;
                        break;
                    case PlaybackStateCode.None:
                        //Indicates that the session has no media to play
                        ToggleMediaControlsVisibility(false);
                        break;

                    default:
                        break;
                }
            });
        }

        void FillWithCompactedActions(OpenNotification openNotification)
        {
            if (openNotification == null) return;

            List<OpenAction> actions = openNotification.Actions;
            int[] compactedViewActionIndices = openNotification.CompactViewActionsIndices;

            int compactedActionsCount = actions.Count - compactedViewActionIndices.Length; //It should be 2 always, Android has only allowd a maximum of 5 actions
            if (actions.Count == 0) return;

            for (int i = 0; i <=  compactedActionsCount; i++) //Only execute two times. (additional actions are always two)
            {
                if (!compactedViewActionIndices.Contains(i))
                {
                    var view= i==0? customAction1 : customAction2; //ensuring we only take the only two views we have available.
                    var action = actions[i];
                    view.SetImageDrawable(action.Icon);
                    view.Tag = action;
                }
            }
        }
        void FillWithCustomAction(ImageButton view, OpenNotification openNotification)
        {
            if (openNotification == null) return;
            var nextCustomAction = mediaSessionCustomActions.FirstOrDefault();
            if (nextCustomAction != null)
            {
                Console.WriteLine($"SETTING CUSTOM ACTION {nextCustomAction.Name}");

                int pixels = (int)Resources.GetDimension(Resource.Dimension.media_widget_secondary_controls_size);

                Drawable drawable = new IconFactory(nextCustomAction.Icon, openNotification.PackageName)
                    .ApplyColorFilter(Color.White)
                    .ResizeDrawable(pixels, pixels)
                    .Build();
               
                mediaSessionCustomActions.Remove(nextCustomAction);

                view.SetImageDrawable(drawable);
                view.Tag= nextCustomAction;
                Console.WriteLine($"(CUSTOM ACTION IS NOT NULL )FILL WITH CUSTOM ACTION: TAG IS NULL? {(view.Tag == null ? "true" : "false")} ");
            }
            else
            {
                
                view.Tag = null;
                Console.WriteLine($"(CUSTOM ACTIOn IS NULL )FILL WITH CUSTOM ACTION: TAG IS NULL? {(view.Tag == null? "true": "false")} ");
            }
        }
        

        private void BindViews(View view)
        {
            title = view.FindViewById<TextView>(Resource.Id.tvSongName);
            album = view.FindViewById<TextView>(Resource.Id.tvAlbumName);
            artist = view.FindViewById<TextView>(Resource.Id.tvArtistName);
            sourceApp = view.FindViewById<TextView>(Resource.Id.sourceapp);

            skipToPrevious = view.FindViewById<ImageButton>(Resource.Id.skip_to_previous);
            playPause = view.FindViewById<ImageButton>(Resource.Id.play_pause);
            skipToNext = view.FindViewById<ImageButton>(Resource.Id.skip_to_next);
            buffering= view.FindViewById<ProgressBar>(Resource.Id.buffering);
            customAction1 = view.FindViewById<ImageButton>(Resource.Id.custom_action_1);
            customAction2 = view.FindViewById<ImageButton>(Resource.Id.custom_action_2);

            repeat= view.FindViewById<ImageButton>(Resource.Id.repeat);
            stop= view.FindViewById<ImageButton>(Resource.Id.stop);

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