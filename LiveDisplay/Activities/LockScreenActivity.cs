namespace LiveDisplay
{
    using Android.Animation;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Content.Res;
    using Android.OS;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using AndroidX.Core.View;
    using LiveDisplay.Activities;
    using LiveDisplay.Fragments;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Awake;
    using LiveDisplay.Services.Wallpaper;
    using System;
    using System.Threading;

    [Activity(Label = "LockScreen",Theme = "@style/LockScreenTheme", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false, LaunchMode = LaunchMode.SingleInstance, ExcludeFromRecents = true)]
    public class LockScreenActivity : AppCompatActivity, View.IOnApplyWindowInsetsListener
    {

        private AndroidX.Fragment.App.Fragment quickGlanceFragment, mediaFragment, notificationFragment;

        private RelativeLayout lockscreen; //The root linear layout, used to implement double tap to sleep.
        private AppCompatImageView lockscreen_wallpaper;
        private float firstTouchTime = -1;
        private float finalTouchTime;
        private readonly float threshold = 1000; //1 second of threshold.(used to implement the double tap.)
        private System.Timers.Timer watchDog; //the watchdog simply will start counting down until it gets resetted by OnUserInteraction() override.
        private TextView welcome;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        protected override void OnNewIntent(Intent intent)
        {
            Console.WriteLine($"(Single Instance)new intent from {(Build.VERSION.SdkInt>= BuildVersionCodes.Q? intent.Identifier: "No identifier")} {intent.Component}");
            base.OnNewIntent(intent);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainActivity.StartCount++;
            SetContentView(Resource.Layout.LockScreen2);
            ThreadPool.QueueUserWorkItem(isApphealthy =>
            {
                if (Checkers.IsNotificationListenerEnabled() == false || Checkers.ThisAppCanPostNotifications() == false)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(Application.Context, GetString(Resource.String.notenoughpermissions), ToastLength.Long).Show();
                        Finish();
                    }
                    );
                }
            });

            Console.WriteLine($"THE COUNT IS {MainActivity.StartCount}");
            
            lockscreen = FindViewById<RelativeLayout>(Resource.Id.main_container);
            lockscreen_wallpaper = FindViewById<AppCompatImageView>(Resource.Id.wallpaper);
            lockscreen.Touch += Lockscreen_Touch;

            watchDog = new System.Timers.Timer
            {
                AutoReset = false
            };

            WallpaperPublisher.NewWallpaperIssued += Wallpaper_NewWallpaperIssued;
            WallpaperPublisher.OnZeroPublishersAvailable += WallpaperPublisher_OnZeroPublishersAvailable;

            
            LoadAllFragments();
            LoadConfiguration();
            Window.DecorView.SetOnApplyWindowInsetsListener(this);
        }

        private void WallpaperPublisher_OnZeroPublishersAvailable(object sender, EventArgs e)
        {
            //lockscreen_wallpaper.SetBackgroundColor(Color.Black);
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
                if (configurationManager.RetrieveAValue(ConfigurationParameters.DisableWallpaperChangeAnim) == false) //If the animation is not disabled.
                {
                    //Animate only when the activity is visible to the user.
                    //Window.DecorView.Animate().SetDuration(100).Alpha(0.5f);
                }

                if (e.Wallpaper != null)
                {
                    //TODO: Offer the user a choice regarding scale type.
                    //Fit XY or Center Crop
                    lockscreen_wallpaper.SetScaleType(ImageView.ScaleType.CenterCrop);
                    lockscreen_wallpaper.SetImageDrawable(e.Wallpaper);
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
                        //ValueAnimator v = ValueAnimator.OfFloat(0, 300);
                        //v.SetInterpolator(new OvershootInterpolator());
                        //v.SetDuration(2000);
                        //v.Start();
                        //v.Update += (sender, e) =>
                        //{
                        //    widgetContainer.SetY((float)e.Animation.AnimatedValue);
                        //};
                       
                        MoveTaskToBack(true);
                    }
                    //Reset the values of touch
                    firstTouchTime = -1;
                    finalTouchTime = -1;
                }
            }
        }

        protected override void OnResume()
        {
            
            AddFlags();
            watchDog.Stop();
            watchDog.Start();
            //if (configurationManager.RetrieveAValue(ConfigurationParameters.TutorialRead) == false)
            //{
            //    welcome = FindViewById<TextView>(Resource.Id.welcomeoverlay);
            //    welcome.Text = Resources.GetString(Resource.String.tutorialtext);
            //    welcome.Visibility = ViewStates.Visible;
            //    welcome.Touch += Welcome_Touch;
            //}
            base.OnResume();
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
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            WallpaperPublisher.NewWallpaperIssued -= Wallpaper_NewWallpaperIssued;
            WallpaperPublisher.OnZeroPublishersAvailable -= WallpaperPublisher_OnZeroPublishersAvailable;
            lockscreen.Touch -= Lockscreen_Touch;
            watchDog.Dispose();
            MainActivity.StartCount--;
            AndroidX.Fragment.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            transaction.Remove(mediaFragment);
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
            watchDog.Stop();
            watchDog.Start();
        }

        public override bool OnKeyLongPress([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
             Console.WriteLine("PRESSED" + e.KeyCode);

            return base.OnKeyLongPress(keyCode, e);
        }
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            Console.WriteLine("KEY DOWN" + e.KeyCode);

            return base.OnKeyDown(keyCode, e);
        }

        private void LoadConfiguration()
        {
            //Load configurations based on User configuration.
            LoadWallpaper(configurationManager);

            int interval = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.TurnOffScreenDelayTime, "5000"));
            watchDog.Interval = interval;
        }

        private void LoadWallpaper(ConfigurationManager configurationManager)
        {
            int savedblurlevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, ConfigurationParameters.DefaultBlurLevel);
            int savedOpacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, ConfigurationParameters.DefaultOpacityLevel);

            try
            {
                //WallpaperManager.GetInstance(Application.Context).ForgetLoadedWallpaper();
                //var wallpaper = WallpaperManager.GetInstance(Application.Context).Drawable;
                //WallpaperPublisher.ChangeWallpaper(
                //    new WallpaperChangedEventArgs
                //    {
                //        Wallpaper = (BitmapDrawable)wallpaper,
                //        OpacityLevel = (short)savedOpacitylevel,
                //        BlurLevel = (short)savedblurlevel,
                //        WallpaperPoster = WallpaperPos
                //        ter.Lockscreen
                //    });
            }
            catch (Exception ex)
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(Application.Context, "You have set the system wallpaper, but the app can't read it, try to change the Wallpaper option again", ToastLength.Long).Show();
                    Console.WriteLine(ex);

                });
            }
        }

        private void LoadAllFragments()
        {
            AndroidX.Fragment.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.WidgetPlaceholder, CreateFragment("media_fragment"), "media_fragment");
            transaction.Add(Resource.Id.mini_widget_container, CreateFragment("quick_glance"), "quick_glance");
            //transaction.Add(Resource.Id.WidgetPlaceholder, CreateFragment("notification_fragment"), "notification_fragment");
            transaction.CommitNow();

        }
        private AndroidX.Fragment.App.Fragment CreateFragment(string tag)
        {
            AndroidX.Fragment.App.Fragment result = null;
            switch (tag)
            {
                case "quick_glance":

                    if (quickGlanceFragment == null)
                    {
                        quickGlanceFragment = new QuickGlanceFragment();
                    }
                    result = quickGlanceFragment;
                    break;
                case "notification_fragment":
                    if (notificationFragment == null)
                    {
                        notificationFragment = new NotificationFragment();
                    }
                    result = notificationFragment;
                    break;
                case "media_fragment":
                    if (mediaFragment == null)
                    {
                        mediaFragment = new MediaFragment();
                    }
                    result = mediaFragment;
                    break;
            }
            return result;
        }

        private void AddFlags()
        {
            WindowInsetsControllerCompat insetsControllerCompat = new WindowInsetsControllerCompat(Window, Window.DecorView);

            insetsControllerCompat.Hide(WindowInsetsCompat.Type.SystemBars());
            insetsControllerCompat.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;

            Window.SetDecorFitsSystemWindows(false);
            if (Build.VERSION.SdkInt <= BuildVersionCodes.O)
            {
                Window.AddFlags(WindowManagerFlags.ShowWhenLocked);
            }
            else 
            { 
                SetShowWhenLocked(true);
            }
        }

        public WindowInsets OnApplyWindowInsets(View v, WindowInsets insets)
        {
            Console.WriteLine(insets.DisplayCutout.SafeInsetTop);
            lockscreen?.SetPadding(0, insets.DisplayCutout.SafeInsetTop, 0, 0);
            return insets;
        }
    }
}