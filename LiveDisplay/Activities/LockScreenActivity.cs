namespace LiveDisplay
{
    using Android.Animation;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Content.Res;
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Provider;
    using Android.Views;
    using Android.Views.Animations;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.RecyclerView.Widget;
    using LiveDisplay.Enums;
    using LiveDisplay.Fragments;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Awake;
    using LiveDisplay.Services.Keyguard;
    using LiveDisplay.Services.Notifications;
    using LiveDisplay.Services.Notifications.NotificationEventArgs;
    using LiveDisplay.Services.Wallpaper;
    using LiveDisplay.Services.Widget;
    using System;
    using System.Threading;

    [Activity(Label = "LockScreen", Theme = "@style/LiveDisplayThemeDark.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false, TaskAffinity = "livedisplay.lockscreen", LaunchMode = LaunchMode.SingleInstance, ExcludeFromRecents = true)]
    public class LockScreenActivity : AppCompatActivity
    {
        private AndroidX.Fragment.App.Fragment clockFragment, musicFragment, notificationFragment;

        private RecyclerView recycler/*, filteredRecyclerView*/;
        private RecyclerView.LayoutManager layoutManager;
        //private Button clearAll;

        private Button startCamera;
        private Button startDialer;
        private FrameLayout lockscreen; //The root linear layout, used to implement double tap to sleep.
        private float firstTouchTime = -1;
        private float finalTouchTime;
        private readonly float threshold = 1000; //1 second of threshold.(used to implement the double tap.)
        private readonly bool REVERSE_LAYOUT_FALSE= false;

        private int halfscreenheight; //To decide the behavior of the double tap.
        private System.Timers.Timer watchDog; //the watchdog simply will start counting down until it gets resetted by OnUserInteraction() override.
        private Animation fadeoutanimation;
        private string doubletapbehavior;
        private ViewPropertyAnimator viewPropertyAnimator;
        private TextView welcome;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.test_lck);
            ThreadPool.QueueUserWorkItem(isApphealthy =>
            {
                if (!Checkers.IsNotificationListenerEnabled()|| !Checkers.IsThisAppADeviceAdministrator())
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(Application.Context, "You dont have the required permissions", ToastLength.Long).Show();
                        Finish();
                    }
                    );
                }
            });

            lockscreen = FindViewById<FrameLayout>(Resource.Id.main_container);
            viewPropertyAnimator = Window.DecorView.Animate();
            viewPropertyAnimator.SetListener(new LockScreenAnimationHelper(Window));

            fadeoutanimation = AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.abc_fade_out);
            fadeoutanimation.AnimationEnd += Fadeoutanimation_AnimationEnd;

            lockscreen.Touch += Lockscreen_Touch;

            watchDog = new System.Timers.Timer
            {
                AutoReset = false
            };
            watchDog.Elapsed += WatchdogInterval_Elapsed;

            halfscreenheight = Resources.DisplayMetrics.HeightPixels / 2;

            WallpaperPublisher.NewWallpaperIssued += Wallpaper_NewWallpaperIssued;

            //CatcherHelper events
            NotificationHijackerWorker.NotificationListSizeChanged += CatcherHelper_NotificationListSizeChanged;

            using (recycler = FindViewById<RecyclerView>(Resource.Id.notification_list))
            {
                using (layoutManager = new LinearLayoutManager(Application.Context, RecyclerView.Horizontal, REVERSE_LAYOUT_FALSE))
                {
                    recycler.SetLayoutManager(layoutManager);
                    recycler.SetAdapter(NotificationHijackerWorker.NotificationAdapter);
                }
            }
            LoadAllFragments();
            LoadConfiguration();

            WallpaperPublisher.CurrentWallpaperCleared += WallpaperPublisher_CurrentWallpaperCleared;
            WidgetStatusPublisher.GetInstance().OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;
        }

        private void LoadActiveFragment()
        {
            WidgetStatusPublisher.GetInstance().ShowActiveWidget();
            
        }
        private void WidgetStatusPublisher_OnWidgetStatusChanged(object sender, WidgetStatusEventArgs e)
        {
            if (e.Show && e.WidgetName != WidgetTypes.CLOCK_FRAGMENT)
            {
                using (var miniclock = FindViewById<TextClock>(Resource.Id.miniclock))
                {
                    miniclock.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                using (var miniclock = FindViewById<TextClock>(Resource.Id.miniclock))
                {
                    miniclock.Visibility = ViewStates.Invisible;
                }
            }
        }

        private void WallpaperPublisher_CurrentWallpaperCleared(object sender, CurrentWallpaperClearedEventArgs e)
        {
            //<document me>
            if (e.PreviousWallpaperPoster == WallpaperPoster.Lockscreen)
                LoadWallpaper(new ConfigurationManager(AppPreferences.Default));
        }

        private void Fadeoutanimation_AnimationEnd(object sender, Animation.AnimationEndEventArgs e)
        {
            var fadeinanimation = AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.abc_fade_in);
            Window.DecorView.StartAnimation(fadeinanimation);
        }

        private void WatchdogInterval_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //it works correctly, but I want to refactor this. (Regression)
            if (ActivityLifecycleHelper.GetInstance().GetActivityState(typeof(LockScreenActivity)) == ActivityStates.Resumed)
                AwakeHelper.TurnOffScreen();
        }

        private void Wallpaper_NewWallpaperIssued(object sender, WallpaperChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (configurationManager.RetrieveAValue(ConfigurationParameters.AwakeCausesBlackWallpaper))
                {
                    Window.DecorView.SetBackgroundColor(Color.Black);
                    return;
                }
                if (configurationManager.RetrieveAValue(ConfigurationParameters.DisableWallpaperChangeAnim) == false) //If the animation is not disabled.
                {
                    if (ActivityLifecycleHelper.GetInstance().GetActivityState(typeof(LockScreenActivity)) == ActivityStates.Resumed)
                    {
                        //Animate only when the activity is visible to the user.
                        Window.DecorView.Animate().SetDuration(100).Alpha(0.5f);
                    }
                }

                if (e.Wallpaper?.Bitmap == null)
                {
                    Window.DecorView.SetBackgroundColor(Color.Black);
                }
                else
                {
                    Window.DecorView.Background = e.Wallpaper;
                }
            });
        }

        private void Lockscreen_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                if (firstTouchTime == -1)
                {
                    firstTouchTime = e.Event.DownTime;
                }
                else if (firstTouchTime != -1)
                {
                    finalTouchTime = e.Event.DownTime;
                    if (firstTouchTime + threshold < finalTouchTime)
                    {
                        firstTouchTime = finalTouchTime; //Let's set the last tap as the first, so the user doesnt have to press twice again
                        return;
                    }
                    else if (firstTouchTime + threshold > finalTouchTime)
                    {
                        //0 Equals: Normal Behavior
                        if (doubletapbehavior == "0")
                        {
                            if (e.Event.RawY < halfscreenheight)
                            {
                                AwakeHelper.TurnOffScreen();
                            }
                            else
                            {
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O && KeyguardHelper.IsDeviceCurrentlyLocked())
                                {
                                    KeyguardHelper.RequestDismissKeyguard(this);
                                }
                                MoveTaskToBack(true);
                            }
                        }
                        //The other value is "1" which means Inverted.
                        else
                        {
                            if (e.Event.RawY < halfscreenheight)
                            {
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O && KeyguardHelper.IsDeviceCurrentlyLocked())
                                {
                                    KeyguardHelper.RequestDismissKeyguard(this);
                                }
                                MoveTaskToBack(true);
                            }
                            else
                            {
                                AwakeHelper.TurnOffScreen();
                            }
                        }
                    }
                    //Reset the values of touch
                    firstTouchTime = -1;
                    finalTouchTime = -1;
                }
            }
        }

        protected override void OnStart()
        {
            base.OnStart(); 
            LoadActiveFragment(); //by this time fragments are ready to react to this call.
        }
        protected override void OnResume()
        {
            base.OnResume();
            AddFlags();
            watchDog.Elapsed += WatchdogInterval_Elapsed;
            watchDog.Stop();
            watchDog.Start();
            ActivityLifecycleHelper.GetInstance().NotifyActivityStateChange(typeof(LockScreenActivity), ActivityStates.Resumed);
            if (!configurationManager.RetrieveAValue(ConfigurationParameters.TutorialRead))
            {
                welcome = FindViewById<TextView>(Resource.Id.welcomeoverlay);
                welcome.Text = Resources.GetString(Resource.String.tutorialtext);
                welcome.Visibility = ViewStates.Visible;
                welcome.Touch += Welcome_Touch;
            }
        }

        private void Welcome_Touch(object sender, View.TouchEventArgs e)
        {
            configurationManager.SaveAValue(ConfigurationParameters.TutorialRead, true);
            if (welcome != null)
            {
                welcome.Visibility = ViewStates.Gone;
                welcome.Touch -= Welcome_Touch;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            watchDog.Stop();
            watchDog.Elapsed -= WatchdogInterval_Elapsed;
            ActivityLifecycleHelper.GetInstance().NotifyActivityStateChange(typeof(LockScreenActivity), ActivityStates.Paused);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ActivityLifecycleHelper.GetInstance().NotifyActivityStateChange(typeof(LockScreenActivity), ActivityStates.Destroyed);

            WallpaperPublisher.NewWallpaperIssued -= Wallpaper_NewWallpaperIssued;
            NotificationHijackerWorker.NotificationListSizeChanged -= CatcherHelper_NotificationListSizeChanged;
            WidgetStatusPublisher.GetInstance().OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;
            lockscreen.Touch -= Lockscreen_Touch;

            watchDog.Stop();
            watchDog.Elapsed -= WatchdogInterval_Elapsed;
            watchDog.Dispose();
            //Dispose Views
            //Views
            recycler.Dispose();
            lockscreen.Dispose();

            viewPropertyAnimator.Dispose();

        }

        public override void OnBackPressed()
        {
            //Do nothing.
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                ThreadPool.QueueUserWorkItem(m =>
                {
                    Thread.Sleep(300);
                    RunOnUiThread(() => AddFlags());
                });
            }
            base.OnWindowFocusChanged(hasFocus);
        }

        //It simply means that a Touch has been registered, no matter where, it was on the lockscreen.
        //used to detect if the user is interacting with the lockscreen.
        public override void OnUserInteraction()
        {
            base.OnUserInteraction();
            //Refresh the Watchdog.
            watchDog.Stop();
            watchDog.Start();
        }

        private void CatcherHelper_NotificationListSizeChanged(object sender, NotificationListSizeChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (e.ThereAreNotifications)
                {
                    //if (clearAll != null)
                    //    clearAll.Visibility = ViewStates.Visible;
                }
                else
                {
                    //if (clearAll != null)
                    //{
                    //    clearAll.Visibility = ViewStates.Invisible;
                    //}
                    if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOffScreenAfterLastNotificationCleared)
                    &&
                    ActivityLifecycleHelper.GetInstance().GetActivityState(typeof(LockScreenActivity)) == ActivityStates.Resumed)
                    {
                        AwakeHelper.TurnOffScreen();
                    }
                }
            });
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            using (NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance())
            {
                notificationSlave.CancelAll();
            }
        }

        private void LoadConfiguration()
        {
            //Load configurations based on User configuration.
            LoadWallpaper(configurationManager);

            int interval = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.TurnOffScreenDelayTime, "5000"));
            watchDog.Interval = interval;
            doubletapbehavior = configurationManager.RetrieveAValue(ConfigurationParameters.DoubleTapOnTopActionBehavior, "0");
            if (configurationManager.RetrieveAValue(ConfigurationParameters.HideShortcutsWhenKeyguardSafe))
            {
                //TODO: Redefine shortcuts into something more useful, they are useless on most phones.
                //if (KeyguardHelper.IsSystemSecured())
                //    using (var shortcuts = FindViewById<FrameLayout>(Resource.Id.shortcutcontainer))
                //    {
                //        //shortcuts.Visibility = ViewStates.Invisible;
                //    }
            }
            else
            {
                //using (var shortcuts = FindViewById<FrameLayout>(Resource.Id.shortcutcontainer))
                //{
                //    if (shortcuts.Visibility != ViewStates.Visible)
                //    {
                //        shortcuts.Visibility = ViewStates.Visible;
                //    }
                //}
            }
        }

        private void LoadWallpaper(ConfigurationManager configurationManager)
        {
            int savedblurlevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, ConfigurationParameters.DefaultBlurLevel);
            int savedOpacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, ConfigurationParameters.DefaultOpacityLevel);

            switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
            {
                case "0":
                    if (Checkers.ThisAppHasReadStoragePermission())
                    {
                        WallpaperManager.GetInstance(Application.Context).ForgetLoadedWallpaper();
                        var wallpaper = WallpaperManager.GetInstance(Application.Context).Drawable;
                        WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = (BitmapDrawable)wallpaper, OpacityLevel = (short)savedOpacitylevel, BlurLevel = (short)savedblurlevel, WallpaperPoster = WallpaperPoster.Lockscreen });
                    }
                    else
                    {
                        RunOnUiThread(() => Toast.MakeText(Application.Context, "You have set the system wallpaper, but the app can't read it, try to change the Wallpaper option again", ToastLength.Long).Show());
                    }
                    break;

                case "1":

                    var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                    if (imagePath != "")
                    {
                        ThreadPool.QueueUserWorkItem(m =>
                        {
                            Bitmap bitmap = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath));
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(savedblurlevel);
                            WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = new BitmapDrawable(Resources, blurImage.GetImageBlur()), OpacityLevel = (short)savedOpacitylevel, BlurLevel = (short)savedblurlevel, WallpaperPoster = WallpaperPoster.Lockscreen });
                        });
                    }
                    break;

                default:
                    Window.DecorView.SetBackgroundColor(Color.Black);
                    break;
            }
        }

        private void LoadAllFragments()
        {
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.fragment_container, CreateFragment(WidgetTypes.CLOCK_FRAGMENT), WidgetTypes.CLOCK_FRAGMENT);
            transaction.Add(Resource.Id.fragment_container, CreateFragment(WidgetTypes.NOTIFICATION_FRAGMENT), WidgetTypes.NOTIFICATION_FRAGMENT);
            transaction.Add(Resource.Id.fragment_container, CreateFragment(WidgetTypes.MUSIC_FRAGMENT), WidgetTypes.MUSIC_FRAGMENT);
            transaction.Commit();
        }

        private AndroidX.Fragment.App.Fragment CreateFragment(string tag)
        {
            AndroidX.Fragment.App.Fragment result = null;
            switch (tag)
            {
                case WidgetTypes.CLOCK_FRAGMENT:

                    if (clockFragment == null)
                    {
                        clockFragment = new ClockFragment();
                    }
                    result = clockFragment;
                    break;

                case WidgetTypes.NOTIFICATION_FRAGMENT:
                    if (notificationFragment == null)
                    {
                        notificationFragment = new NotificationFragment();
                    }
                    result = notificationFragment;
                    break;

                case WidgetTypes.MUSIC_FRAGMENT:
                    if (musicFragment == null)
                    {
                        musicFragment = new MusicFragment();
                    }
                    result = musicFragment;
                    break;
            }
            return result;
        }

        private void AddFlags()
        {
            using (var view = Window.DecorView)
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.OMr1)
                    Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;

                var uiOptions = (int)view.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                newUiOptions |= (int)SystemUiFlags.Immersive;
                // This option will make bars disappear by themselves
                newUiOptions |= (int)SystemUiFlags.ImmersiveSticky;
                view.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
                Window.AddFlags(WindowManagerFlags.DismissKeyguard);
                Window.AddFlags(WindowManagerFlags.ShowWhenLocked);
            }
        }
    }

    public class LockScreenAnimationHelper : Java.Lang.Object, Animator.IAnimatorListener
    {
        private readonly Window lockScreenWindow;

        public LockScreenAnimationHelper(Window window)
        {
            lockScreenWindow = window;
        }

        public void OnAnimationCancel(Animator animation) { /*this method is not needed*/ }
        public void OnAnimationEnd(Animator animation)
        {
            lockScreenWindow.DecorView.Animate().SetDuration(100).Alpha(1f);
        }
        public void OnAnimationRepeat(Animator animation) { /*this method is not needed*/ }
        public void OnAnimationStart(Animator animation) { /*this method is not needed*/ }
    }
}