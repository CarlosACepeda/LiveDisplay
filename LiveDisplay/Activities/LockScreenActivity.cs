namespace LiveDisplay
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Content.Res;
    using Android.Graphics;
    using Android.OS;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using AndroidX.Activity.Result;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using AndroidX.Core.View;
    using Google.Android.Material.FloatingActionButton;
    using Google.Android.Material.Snackbar;
    using LiveDisplay.Activities;
    using LiveDisplay.Fragments;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Configuration;
    using LiveDisplay.Services.Wallpaper;
    using LiveDisplay.Visualizers;
    using System;
    using System.Threading;
    using PopupMenu = AndroidX.AppCompat.Widget.PopupMenu;

    [Activity(Label = "LockScreen",
        Theme = "@style/LockScreenTheme",
        ScreenOrientation = ScreenOrientation.Sensor,
        ConfigurationChanges =
        ConfigChanges.Navigation
        | ConfigChanges.KeyboardHidden
        | ConfigChanges.UiMode
        | ConfigChanges.Orientation
        | ConfigChanges.ScreenSize,
        LaunchMode = LaunchMode.SingleInstance,
        WindowSoftInputMode = SoftInput.AdjustPan)]
    public class LockScreenActivity : AppCompatActivity, View.IOnApplyWindowInsetsListener, PopupMenu.IOnMenuItemClickListener, IActivityResultCallback
    {

        private AndroidX.Fragment.App.Fragment quickGlanceFragment, mediaFragment;
        private HideableFragment notificationFragment;

        private RelativeLayout lockscreen; //The root linear layout, used to implement double tap to sleep.
        private AppCompatImageView lockscreen_wallpaper;
        private long firstTouchTime = -1;
        private long finalTouchTime;
        private const long DoubleTapThreshold = 1000; //1 second of threshold.(used to implement the double tap.)
        private TextView welcome;
        private FloatingActionButton quickSettings;
        private LinearLayout visualizerContainer;
        private readonly ConfigurationManager configurationManager = new ConfigurationManager();

        private KeyguardPendingIntentMediator pendingIntentMediator;

        protected override void OnNewIntent(Intent intent)
        {
            Console.WriteLine($"(Single Instance)new intent from {(Build.VERSION.SdkInt >= BuildVersionCodes.Q ? intent.Identifier : "No identifier")} {intent.Component}");
            base.OnNewIntent(intent);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainActivity.StartCount++;
            SetContentView(Resource.Layout.LockScreen2);
            ThreadPool.QueueUserWorkItem(isApphealthy =>
            {
                if (!Checkers.AreMandatoryPermissionsEnabled())
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(Application.Context, GetString(Resource.String.not_enough_permissions), ToastLength.Long).Show();
                        Finish();
                    }
                    );
                }
            });

            Console.WriteLine($"THE COUNT IS {MainActivity.StartCount}");

            lockscreen = FindViewById<RelativeLayout>(Resource.Id.main_container);
            lockscreen_wallpaper = FindViewById<AppCompatImageView>(Resource.Id.wallpaper);
            quickSettings = FindViewById<FloatingActionButton>(Resource.Id.quick_settings);
            visualizerContainer = FindViewById<LinearLayout>(Resource.Id.visualizer_container);

            lockscreen.Click += Lockscreen_Click;
            lockscreen.Touch += Lockscreen_Touch;
            quickSettings.Click += QuickSettings_Click;
            WallpaperPublisher.NewWallpaperIssued += Wallpaper_NewWallpaperIssued;
            WallpaperPublisher.OnZeroPublishersAvailable += WallpaperPublisher_OnZeroPublishersAvailable;
            SharedPreferenceListenerService.ConfigurationChanged += SharedPreferenceListenerService_ConfigurationChanged;
            pendingIntentMediator = KeyguardPendingIntentMediator.GetInstance();
            pendingIntentMediator.RequiredSetActivityToBeCalled += LockScreenActivity_RequiredSetActivityToBeCalled;

            QuickGlanceFragment.ShowMessagesButtonClicked += QuickGlanceFragment_ShowMessagesButtonClicked;

            LoadAllFragments();
            LoadConfiguration();
            Window.DecorView.SetOnApplyWindowInsetsListener(this);
        }

        private void Lockscreen_Touch(object sender, View.TouchEventArgs e)
        {
            //Click outside fragment implementation, used to dismiss fragments.
            if (e.Event.Action == MotionEventActions.Up)
            {
                if (notificationFragment?.IsHidden == false)
                {
                    // create a rect for storing the fragment window rect
                    Rect r = new Rect(0, 0, 0, 0);
                    // retrieve the fragment's windows rect
                    notificationFragment.View.GetHitRect(r);
                    // check if the event position is inside the window rect
                    bool intersects = r.Contains((int)e.Event.GetX(), (int)e.Event.GetY());
                    // if the event is not inside then we can close the fragment
                    if (!intersects)
                    {
                        SupportFragmentManager.BeginTransaction()
                            .Hide(notificationFragment)
                            .SetTransition(AndroidX.Fragment.App.FragmentTransaction.TransitFragmentFade)
                            .CommitNow();
                    }
                }
            }
            e.Handled = false;
        }

        private void QuickGlanceFragment_ShowMessagesButtonClicked(object sender, EventArgs e)
        {
            AndroidX.Fragment.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();

            notificationFragment ??= new NotificationFragment();

            if (!SupportFragmentManager.IsDestroyed)
            {
                if (!notificationFragment.IsAdded)
                {
                    transaction.Add(Resource.Id.WidgetPlaceholder, notificationFragment, "notification_fragment")
                        .CommitNow();
                }
            }
        }

        private void QuickSettings_Click(object sender, EventArgs e)
        {
            PopupMenu quickSettingsMenu = new PopupMenu(this, (View)sender);
            quickSettingsMenu.MenuInflater.Inflate(Resource.Menu.quick_settings_menu, quickSettingsMenu.Menu);
            quickSettingsMenu.SetOnMenuItemClickListener(this);
            quickSettingsMenu.Show();
        }

        private void Lockscreen_Click(object sender, EventArgs e)
        {
            if (firstTouchTime == -1)
            {
                firstTouchTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            }
            else if (firstTouchTime != -1)
            {
                finalTouchTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (firstTouchTime + DoubleTapThreshold < finalTouchTime)
                {
                    firstTouchTime = finalTouchTime; //Let's set the last tap as the first, so the user doesnt have to press twice again
                    return;
                }
                else if (firstTouchTime + DoubleTapThreshold > finalTouchTime)
                {
                    //ValueAnimator v = ValueAnimator.OfFloat(0, 300);
                    //v.SetInterpolator(new OvershootInterpolator());
                    //v.SetDuration(2000);
                    //v.Start();
                    //v.Update += (sender, e) =>
                    //{
                    //    widgetContainer.SetY((float)e.Animation.AnimatedValue);
                    //};

                    MoveTaskToBack(false);
                }
                //Reset the values of touch
                firstTouchTime = -1;
                finalTouchTime = -1;
            }

        }

        private void LockScreenActivity_RequiredSetActivityToBeCalled(object sender, KeyguardPendingIntentMediator e)
        {
            e.SetActivity(this);
        }

        private void SharedPreferenceListenerService_ConfigurationChanged(object sender, Services.Configuration.ConfigurationChangedEventArgs e)
        {
            if (e.Key == ConfigurationParameters.WallpaperScaleType)
            {
                int centerCrop = Resources.GetInteger(Resource.Integer.center_crop);
                int fitXy = Resources.GetInteger(Resource.Integer.fit_xy);
                if ((int)e.Value == centerCrop)
                {
                    lockscreen_wallpaper.SetScaleType(ImageView.ScaleType.CenterCrop);
                }
                else if ((int)e.Value == fitXy)
                {
                    lockscreen_wallpaper.SetScaleType(ImageView.ScaleType.FitXy);
                }
            }
            if (e.Key == ConfigurationParameters.UseWhenNoMediaPresent)
            {
                if ((bool)e.Value)
                {
                    var snackbar = Snackbar.Make(lockscreen, Resource.String.using_when_no_media_present, Snackbar.LengthLong);
                    snackbar.SetAnchorView(quickSettings);
                    snackbar.Show();
                }
                else
                {
                    var snackbar = Snackbar.Make(lockscreen, Resource.String.using_only_when_media_present, Snackbar.LengthLong);
                    snackbar.SetAnchorView(quickSettings);
                    snackbar.Show();
                }
            }
            if(e.Key== ConfigurationParameters.CurrentVisualizerStyle)
            {
                SetVisualizer((string)e.Value);
            }
        }

        private void SetVisualizer(string visualizer)
        {
            visualizerContainer.RemoveAllViews();
            switch (visualizer)
            {

                case CircleVisualizerView.Name:
                    {
                        var v = new CircleVisualizerView(this, Color.White);
                        Console.WriteLine("Cicled to Circle");
                        visualizerContainer.AddView(v);
                    }
                    break;
                case BarVisualizerView.Name:
                    {
                        var v = new BarVisualizerView(this, Color.White);
                        visualizerContainer.AddView(v);
                        Console.WriteLine("Cicled to Bar");
                    }
                    break;
            }
        }

        private void WallpaperPublisher_OnZeroPublishersAvailable(object sender, EventArgs e)
        {
            //lockscreen_wallpaper.SetBackgroundColor(Color.Black);
        }
        private void Wallpaper_NewWallpaperIssued(object sender, WallpaperChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (e.Wallpaper != null)
                {
                    int fitXy = Resources.GetInteger(Resource.Integer.fit_xy);
                    int centerCrop = Resources.GetInteger(Resource.Integer.center_crop);

                    if (configurationManager.RetrieveAValue(ConfigurationParameters.WallpaperScaleType, centerCrop) == centerCrop)
                        lockscreen_wallpaper.SetScaleType(ImageView.ScaleType.CenterCrop);
                    else
                        lockscreen_wallpaper.SetScaleType(ImageView.ScaleType.FitXy);


                    lockscreen_wallpaper.SetImageDrawable(e.Wallpaper);
                }
            });
        }
        protected override void OnResume()
        {
            LoadVisualizer();
            AddFlags();
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

        protected override void OnDestroy()
        {
            WallpaperPublisher.NewWallpaperIssued -= Wallpaper_NewWallpaperIssued;
            WallpaperPublisher.OnZeroPublishersAvailable -= WallpaperPublisher_OnZeroPublishersAvailable;
            MainActivity.StartCount--;
            AndroidX.Fragment.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            transaction.Remove(mediaFragment);
            transaction.Remove(quickGlanceFragment);
            transaction.CommitNowAllowingStateLoss();

            pendingIntentMediator.RequiredSetActivityToBeCalled -= LockScreenActivity_RequiredSetActivityToBeCalled;
            SharedPreferenceListenerService.ConfigurationChanged -= SharedPreferenceListenerService_ConfigurationChanged;

            base.OnDestroy();

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
        }

        private void LoadVisualizer()
        {
            string currentVisualizer = VisualizerService.GetInstance().CurrentVisualizerStyle;
            if (currentVisualizer != string.Empty)
            {
                SetVisualizer(currentVisualizer);
            }
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

            transaction.CommitNow();

        }
        private AndroidX.Fragment.App.Fragment CreateFragment(string tag)
        {
            AndroidX.Fragment.App.Fragment result = null;
            switch (tag)
            {
                case "quick_glance":

                    quickGlanceFragment ??= new QuickGlanceFragment();
                    result = quickGlanceFragment;
                    break;
                case "notification_fragment":
                    notificationFragment ??= new NotificationFragment();
                    result = notificationFragment;
                    break;
                case "media_fragment":
                    mediaFragment ??= new MediaFragment();
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

        public bool OnMenuItemClick(IMenuItem item)
        {
            int id = item.ItemId;
            switch (id)
            {
                case Resource.Id.toggle_wallpaper_appearance:

                    int centerCrop = Resources.GetInteger(Resource.Integer.center_crop);

                    if (configurationManager.RetrieveAValue(ConfigurationParameters.WallpaperScaleType, centerCrop) == centerCrop)
                        configurationManager.SaveAValue(ConfigurationParameters.WallpaperScaleType, Resources.GetInteger(Resource.Integer.fit_xy));
                    else
                        configurationManager.SaveAValue(ConfigurationParameters.WallpaperScaleType, Resources.GetInteger(Resource.Integer.center_crop));

                    return true;

                case Resource.Id.toggle_show_on_lock_screen:
                    if (configurationManager.RetrieveAValue(ConfigurationParameters.UseWhenNoMediaPresent) == false)
                        configurationManager.SaveAValue(ConfigurationParameters.UseWhenNoMediaPresent, true);
                    else
                        configurationManager.SaveAValue(ConfigurationParameters.UseWhenNoMediaPresent, false);
                    break;
                case Resource.Id.cycle_visualization:
                    VisualizerService.GetInstance().CycleVisualizerStyle();
                    break;
                case Resource.Id.go_to_full_settings:
                    using (Intent intent = new Intent(this, typeof(SettingsActivity)))
                    {
                        KeyguardPendingIntentMediator.GetInstance().SendIntent(intent);
                    }
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
        public void OnActivityResult(Java.Lang.Object result)
        {
            Console.WriteLine("result");
        }
    }
}