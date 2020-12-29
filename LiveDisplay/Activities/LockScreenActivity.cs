namespace LiveDisplay
{
    using Android.Animation;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Content.Res;
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.Media;
    using Android.Media.Session;
    using Android.OS;
    using Android.Provider;
    using Android.Util;
    using Android.Views;
    using Android.Views.Animations;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.RecyclerView.Widget;
    using LiveDisplay.Adapters;
    using LiveDisplay.Fragments;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using LiveDisplay.Servicios.Awake;
    using LiveDisplay.Servicios.FloatingNotification;
    using LiveDisplay.Servicios.Keyguard;
    using LiveDisplay.Servicios.Music;
    using LiveDisplay.Servicios.Notificaciones;
    using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
    using LiveDisplay.Servicios.Wallpaper;
    using LiveDisplay.Servicios.Widget;
    using System;
    using System.Threading;

    [Activity(Label = "LockScreen", Theme = "@style/LiveDisplayThemeDark.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false, TaskAffinity = "livedisplay.lockscreen", LaunchMode = LaunchMode.SingleInstance, ExcludeFromRecents = true)]
    public class LockScreenActivity : AppCompatActivity
    {

        private AndroidX.Fragment.App.Fragment clockFragment, musicFragment, notificationFragment;

        private RecyclerView recycler/*, filteredRecyclerView*/;
        private RecyclerView.LayoutManager layoutManager;

        //private ImageView unlocker;
        private Button clearAll;

        private TextView livedisplayinfo;
        private Button startCamera;
        private Button startDialer;
        private LinearLayout lockscreen; //The root linear layout, used to implement double tap to sleep.
        private float firstTouchTime = -1;
        private float finalTouchTime;
        private readonly float threshold = 1000; //1 second of threshold.(used to implement the double tap.)
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
            SetContentView(Resource.Layout.LockScreen2);
            ThreadPool.QueueUserWorkItem(isApphealthy =>
            {
                if (Checkers.IsNotificationListenerEnabled() == false || Checkers.IsThisAppADeviceAdministrator() == false)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(Application.Context, "You dont have the required permissions", ToastLength.Long).Show();
                        Finish();
                    }
                    );
                }
            });
            
            startCamera = FindViewById<Button>(Resource.Id.btnStartCamera);
            startDialer = FindViewById<Button>(Resource.Id.btnStartPhone);
            clearAll = FindViewById<Button>(Resource.Id.btnClearAllNotifications);
            lockscreen = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            viewPropertyAnimator = Window.DecorView.Animate();
            viewPropertyAnimator.SetListener(new LockScreenAnimationHelper(Window));
            livedisplayinfo = FindViewById<TextView>(Resource.Id.livedisplayinfo);
            livedisplayinfo.Visibility = ViewStates.Gone;  //You won't be seeing this anymore.

            fadeoutanimation = AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.abc_fade_out);
            fadeoutanimation.AnimationEnd += Fadeoutanimation_AnimationEnd;

            clearAll.Click += BtnClearAll_Click;
            startCamera.Click += StartCamera_Click;
            startDialer.Click += StartDialer_Click;
            lockscreen.Touch += Lockscreen_Touch;

            watchDog = new System.Timers.Timer
            {
                AutoReset = false
            };
            watchDog.Elapsed += WatchdogInterval_Elapsed;

            halfscreenheight = Resources.DisplayMetrics.HeightPixels / 2;

            WallpaperPublisher.NewWallpaperIssued += Wallpaper_NewWallpaperIssued;

            //CatcherHelper events
            CatcherHelper.NotificationListSizeChanged += CatcherHelper_NotificationListSizeChanged;

            using (recycler = FindViewById<RecyclerView>(Resource.Id.NotificationListRecyclerView))
            {
                using (layoutManager = new LinearLayoutManager(Application.Context))
                {
                    recycler.SetLayoutManager(layoutManager);
                    recycler.SetAdapter(CatcherHelper.notificationAdapter);
                }
            }
            LoadAllFragments();
            LoadConfiguration();

            WallpaperPublisher.CurrentWallpaperCleared += WallpaperPublisher_CurrentWallpaperCleared;
            WidgetStatusPublisher.OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;
        }

        private void WidgetStatusPublisher_OnWidgetStatusChanged(object sender, WidgetStatusEventArgs e)
        {
            if (e.Show && e.WidgetName != "ClockFragment")
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
            GC.Collect();
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
                                //Finish();
                                //using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(TransparentActivity))))
                                //{
                                //    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.);
                                //    StartActivity(intent);
                                //}
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                    if (KeyguardHelper.IsDeviceCurrentlyLocked())
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
                                //Finish();
                                //using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(TransparentActivity))))
                                //{
                                //    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.MultipleTask);
                                //    StartActivity(intent);
                                //}
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                    if (KeyguardHelper.IsDeviceCurrentlyLocked())
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

        protected override void OnResume()
        {
            base.OnResume();
            AddFlags();
            watchDog.Elapsed += WatchdogInterval_Elapsed;
            watchDog.Stop();
            watchDog.Start();
            ActivityLifecycleHelper.GetInstance().NotifyActivityStateChange(typeof(LockScreenActivity), ActivityStates.Resumed);
            if (configurationManager.RetrieveAValue(ConfigurationParameters.TutorialRead) == false)
            {
                welcome = FindViewById<TextView>(Resource.Id.welcomeoverlay);
                welcome.Text = Resources.GetString(Resource.String.tutorialtext);
                welcome.Visibility = ViewStates.Visible;
                welcome.Touch += Welcome_Touch;
            }            
            //Check if Awake is enabled.
            //Refactor
            switch (AwakeHelper.GetAwakeStatus())
            {
                case AwakeStatus.None:
                    livedisplayinfo.Text = Resources.GetString(Resource.String.idk);
                    break;
                case AwakeStatus.CompletelyDisabled:
                    livedisplayinfo.Text = "Completely disabled";
                    break;
                case AwakeStatus.Up:
                    livedisplayinfo.Text = "Awake is active";
                    break;
                case AwakeStatus.Sleeping:
                    livedisplayinfo.Text = "Awake is Sleeping";
                    break;
                case AwakeStatus.UpWithDeviceMotionDisabled:
                    livedisplayinfo.Text = "Awake is active but not listening orientation changes";
                    break;
                case AwakeStatus.SleepingWithDeviceMotionEnabled:
                    livedisplayinfo.Text = "Awake is sleeping but listening orientation changes";
                    break;
                case AwakeStatus.DisabledbyUser:
                    livedisplayinfo.Text = "Awake is disabled by the user.";
                    break;
                default:
                    break;
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
            GC.Collect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ActivityLifecycleHelper.GetInstance().NotifyActivityStateChange(typeof(LockScreenActivity), ActivityStates.Destroyed);
            //Unbind events

            //unlocker.Touch -= Unlocker_Touch;
            clearAll.Click -= BtnClearAll_Click;
            WallpaperPublisher.NewWallpaperIssued -= Wallpaper_NewWallpaperIssued;
            CatcherHelper.NotificationListSizeChanged -= CatcherHelper_NotificationListSizeChanged;
            WidgetStatusPublisher.OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;
            lockscreen.Touch -= Lockscreen_Touch;

            watchDog.Stop();
            watchDog.Elapsed -= WatchdogInterval_Elapsed;
            watchDog.Dispose();
            //Dispose Views
            //Views
            recycler.Dispose();
            //unlocker.Dispose();
            clearAll.Dispose();
            lockscreen.Dispose();
            //wallpaperView.Background?.Dispose();
            //wallpaperView = null;

            viewPropertyAnimator.Dispose();

            //Dispose Fragments
            livedisplayinfo?.Dispose();
        }

        public override void OnBackPressed()
        {
            //Do nothing.
            //In Nougat it works after several tries to go back, I can't fix that.
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (hasFocus == false)
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
            //Refresh the Watchdog, damn dog, annoying, lol.
            watchDog.Stop();
            watchDog.Start();
        }

        private void CatcherHelper_NotificationListSizeChanged(object sender, NotificationListSizeChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (e.ThereAreNotifications)
                {
                    if (clearAll != null)
                        clearAll.Visibility = ViewStates.Visible;
                }
                else
                {
                    if (clearAll != null)
                    {
                        clearAll.Visibility = ViewStates.Invisible;
                    }
                    if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOffScreenAfterLastNotificationCleared)
                    &&
                    ActivityLifecycleHelper.GetInstance().GetActivityState(typeof(LockScreenActivity))== ActivityStates.Resumed)
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
        private void StartCamera_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent(MediaStore.IntentActionStillImageCamera))
            {
                StartActivity(intent);
            }
        }

        private void StartDialer_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent(Intent.ActionDial))
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    if (KeyguardHelper.IsDeviceCurrentlyLocked())
                    {
                        KeyguardHelper.RequestDismissKeyguard(this);
                    }
                StartActivity(intent);
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
                if(KeyguardHelper.IsSystemSecured())
                using (var shortcuts = FindViewById<FrameLayout>(Resource.Id.shortcutcontainer))
                {
                    shortcuts.Visibility = ViewStates.Invisible;
                }
            }
            else 
            {
                using (var shortcuts = FindViewById<FrameLayout>(Resource.Id.shortcutcontainer))
                {
                    if (shortcuts.Visibility != ViewStates.Visible)
                    {
                        shortcuts.Visibility = ViewStates.Visible;
                    }
                }
            }
        }

        private void LoadWallpaper(ConfigurationManager configurationManager)
        {
            int savedblurlevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, ConfigurationParameters.DefaultBlurLevel);
            int savedOpacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, ConfigurationParameters.DefaultOpacityLevel);

            switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
            {
                case "0":

                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = null, WallpaperPoster = WallpaperPoster.Lockscreen });
                    break;

                case "1":
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

                case "2":

                    var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                    if (imagePath != "")
                    {
                        ThreadPool.QueueUserWorkItem(m =>
                        {
                            Bitmap bitmap = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath));
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(savedblurlevel);
                            Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = new BitmapDrawable(Resources, bitmap), OpacityLevel = (short)savedOpacitylevel, BlurLevel = (short)savedblurlevel, WallpaperPoster = WallpaperPoster.Lockscreen });
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
            AndroidX.Fragment.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.WidgetPlaceholder, CreateFragment("clock_fragment"), "clock_fragment");
            transaction.Add(Resource.Id.WidgetPlaceholder, CreateFragment("notification_fragment"), "notification_fragment");
            transaction.Add(Resource.Id.WidgetPlaceholder, CreateFragment("music_fragment"), "music_fragment");
            transaction.Commit();

        }
        private AndroidX.Fragment.App.Fragment CreateFragment(string tag)
        {
            AndroidX.Fragment.App.Fragment result = null;
            switch (tag)
            {
                case "clock_fragment":

                    if (clockFragment == null)
                    {
                        clockFragment = new ClockFragment();
                    }
                    result = clockFragment;
                    break;
                case "notification_fragment":
                    if (notificationFragment == null)
                    {
                        notificationFragment = new NotificationFragment();
                    }
                    result = notificationFragment;
                    break;
                case "music_fragment":
                    if (musicFragment == null)
                    {
                        musicFragment = new MusicFragment();
                    }
                    result = musicFragment;
                    break;
            }
            Log.Debug("LiveDisplay", "create: " + result.ToString());
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
        private Window lockScreenWindow;

        public LockScreenAnimationHelper(Window window)
        {
            lockScreenWindow = window;
        }

        public void OnAnimationCancel(Animator animation)
        {
        }

        public void OnAnimationEnd(Animator animation)
        {
            lockScreenWindow.DecorView.Animate().SetDuration(100).Alpha(1f);
        }

        public void OnAnimationRepeat(Animator animation)
        {
        }

        public void OnAnimationStart(Animator animation)
        {
        }
    }
}