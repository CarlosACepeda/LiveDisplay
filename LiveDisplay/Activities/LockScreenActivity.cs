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
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.RecyclerView.Widget;
    using LiveDisplay.Activities;
    using LiveDisplay.Fragments;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using LiveDisplay.Servicios.Awake;
    using LiveDisplay.Servicios.Wallpaper;
    using System;
    using System.Threading;

    [Activity(Label = "LockScreen",Theme = "@style/LiveDisplayThemeDark.NoActionBar", ShowWhenLocked = true, ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false, LaunchMode = LaunchMode.SingleInstance, ExcludeFromRecents = true)]
    public class LockScreenActivity : AppCompatActivity
    {

        private AndroidX.Fragment.App.Fragment clockFragment, musicFragment, notificationFragment;

        private LinearLayout lockscreen; //The root linear layout, used to implement double tap to sleep.
        private float firstTouchTime = -1;
        private float finalTouchTime;
        private readonly float threshold = 1000; //1 second of threshold.(used to implement the double tap.)
        private System.Timers.Timer watchDog; //the watchdog simply will start counting down until it gets resetted by OnUserInteraction() override.
        private ViewPropertyAnimator viewPropertyAnimator;
        private TextView welcome;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        protected override void OnNewIntent(Intent intent)
        {
            Console.WriteLine($"(Single Instance)new intent from {(Build.VERSION.SdkInt>= BuildVersionCodes.Q? intent.Identifier: "No identifier")} {intent.Component}");
            base.OnNewIntent(intent);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if(Build.VERSION.SdkInt>= BuildVersionCodes.Q)
            {
                SetShowWhenLocked(true);
            }
            base.OnCreate(savedInstanceState);
            MainActivity.StartCount++;
            SetContentView(Resource.Layout.LockScreen2);
            ThreadPool.QueueUserWorkItem(isApphealthy =>
            {
                if (Checkers.IsNotificationListenerEnabled() == false || Checkers.ThisAppCanPostNotifications() == false)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(Application.Context, "You dont have the required permissions", ToastLength.Long).Show();
                        Finish();
                    }
                    );
                }
            });

            Console.WriteLine($"THE COUNT IS {MainActivity.StartCount}");
            
            lockscreen = FindViewById<LinearLayout>(Resource.Id.main_container);
            lockscreen.Touch += Lockscreen_Touch;

            watchDog = new System.Timers.Timer
            {
                AutoReset = false
            };

            WallpaperPublisher.NewWallpaperIssued += Wallpaper_NewWallpaperIssued;
            
            
            LoadAllFragments();
            LoadConfiguration();
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
                    Window.DecorView.Animate().SetDuration(100).Alpha(0.5f);
                }

                if (e.Wallpaper == null)
                {
                    Window.DecorView.SetBackgroundColor(Color.Black);
                }
                else
                {
                    Window.DecorView.SetBackgroundColor(Color.Black);
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

                        MoveTaskToBack(true);

                        ////0 Equals: Normal Behavior
                        //if (doubletapbehavior == "0")
                        //{
                        //    if (e.Event.RawY < halfscreenheight)
                        //    {
                        //        AwakeHelper.TurnOffScreen();
                        //    }
                        //    else
                        //    {
                        //        //Finish();
                        //        //using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(TransparentActivity))))
                        //        //{
                        //        //    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.);
                        //        //    StartActivity(intent);
                        //        //}
                        //        MoveTaskToBack(true);
                        //    }
                        //}
                        ////The other value is "1" which means Inverted.
                        //else
                        //{
                        //    if (e.Event.RawY < halfscreenheight)
                        //    {
                        //        //Finish();
                        //        //using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(TransparentActivity))))
                        //        //{
                        //        //    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.MultipleTask);
                        //        //    StartActivity(intent);
                        //        //}
                        //        MoveTaskToBack(true);

                        //        //try
                        //        //{
                        //        //    ValueAnimator valueAnimator = ValueAnimator.OfFloat(0, 100);
                        //        //    valueAnimator.SetDuration(1000);
                        //        //    valueAnimator.Start();
                        //        //    valueAnimator.Update += (sender, e) =>
                        //        //    {
                        //        //        musicFragment.View.SetY((float)e.Animation.AnimatedValue);
                        //        //    };
                        //        //}
                        //        //catch (Exception ex)
                        //        //{
                        //        //    Console.WriteLine(ex);
                        //        //}
                                
                        //    }
                        //    else
                        //    {
                        //        AwakeHelper.TurnOffScreen();
                        //    }
                        //}
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
            watchDog.Stop();
            watchDog.Start();
            if (configurationManager.RetrieveAValue(ConfigurationParameters.TutorialRead) == false)
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
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            WallpaperPublisher.NewWallpaperIssued -= Wallpaper_NewWallpaperIssued;
            lockscreen.Touch -= Lockscreen_Touch;
            watchDog.Dispose();
            MainActivity.StartCount--;
            AndroidX.Fragment.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            transaction.Remove(musicFragment);
            viewPropertyAnimator.Dispose();
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
                WallpaperManager.GetInstance(Application.Context).ForgetLoadedWallpaper();
                var wallpaper = WallpaperManager.GetInstance(Application.Context).Drawable;
                WallpaperPublisher.ChangeWallpaper(
                    new WallpaperChangedEventArgs
                    {
                        Wallpaper = (BitmapDrawable)wallpaper,
                        OpacityLevel = (short)savedOpacitylevel,
                        BlurLevel = (short)savedblurlevel,
                        WallpaperPoster = WallpaperPoster.Lockscreen
                    });
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
                        musicFragment = new MediaFragment();
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
}