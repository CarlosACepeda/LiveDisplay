using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.JackAndPhantom;
using LiveDisplay.Activities;
using LiveDisplay.Activities.ActivitiesEventArgs;
using LiveDisplay.Fragments;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.FloatingNotification;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", Theme = "@style/LiveDisplayThemeDark", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait, TaskAffinity = "livedisplay.lockscreen", LaunchMode = LaunchMode.SingleInstance, ExcludeFromRecents = true)]
    public class LockScreenActivity : Activity
    {
        private RecyclerView recycler;
        private RecyclerView.LayoutManager layoutManager;
        private ImageView wallpaperView;
        //private ImageView unlocker;
        private Button clearAll;
        private NotificationFragment notificationFragment;
        private MusicFragment musicFragment;
        private ClockFragment clockFragment;
        private WeatherFragment weatherFragment;
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
        private bool isMusicWidgetPresent;

        public static event EventHandler<LockScreenLifecycleEventArgs> OnActivityStateChanged;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LockScreen);
            Window.DecorView.SetBackgroundColor(Color.Black);
            ThreadPool.QueueUserWorkItem(isApphealthy =>
            {
                bool canDrawOverlays = true;
                if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1) //In Lollipop and less this permission is granted at Install time.
                {
                    canDrawOverlays = Checkers.ThisAppCanDrawOverlays();
                }

                if (Checkers.IsNotificationListenerEnabled() == false || canDrawOverlays == false || Checkers.IsThisAppADeviceAdministrator() == false)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(Application.Context, "You dont have the required permissions", ToastLength.Long).Show();
                        Finish();
                    }
                    );
                    
                }
            });

            //Views
            wallpaperView = FindViewById<ImageView>(Resource.Id.wallpaper);
            //unlocker = FindViewById<ImageView>(Resource.Id.unlocker);
            startCamera = FindViewById<Button>(Resource.Id.btnStartCamera);
            startDialer = FindViewById<Button>(Resource.Id.btnStartPhone);
            clearAll = FindViewById<Button>(Resource.Id.btnClearAllNotifications);
            lockscreen = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            fadeoutanimation = AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.abc_fade_out);
            fadeoutanimation.AnimationEnd += Fadeoutanimation_AnimationEnd;

            notificationFragment = new NotificationFragment();
            musicFragment = new MusicFragment();
            clockFragment = new ClockFragment();
            weatherFragment = new WeatherFragment();
            clearAll.Click += BtnClearAll_Click;
            //unlocker.Touch += Unlocker_Touch;
            startCamera.Click += StartCamera_Click;
            startDialer.Click += StartDialer_Click;
            lockscreen.Touch += Lockscreen_Touch;

            watchDog = new System.Timers.Timer();
            watchDog.AutoReset = false;
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

            LoadClockFragment();

            LoadNotificationFragment();

            LoadConfiguration();

            CheckNotificationListSize();
        }

        private void Fadeoutanimation_AnimationEnd(object sender, Animation.AnimationEndEventArgs e)
        {
            var fadeinanimation = AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.abc_fade_in);
            wallpaperView.StartAnimation(fadeinanimation);
        }

        private void WatchdogInterval_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Awake.TurnOffScreen();
        }

        private void MusicController_MusicStopped(object sender, EventArgs e)
        {
            StopMusicController();
        }

        private void MusicController_MusicPaused(object sender, EventArgs e)
        {
            CheckIfMusicIsStillPaused();
        }

        private void CheckIfMusicIsStillPaused()
        {
            ThreadPool.QueueUserWorkItem(method =>
            {
                Thread.Sleep(5000); //Wait 5 seconds.

                if (MusicController.MusicStatus != PlaybackStateCode.Playing)
                {
                    RunOnUiThread(() =>
                    {
                        StopMusicController();
                        StopFloatingNotificationService();
                        Log.Info("LiveDisplay", "Musicfragment stopped");
                    });
                }
            });
        }

        private void MusicController_MusicPlaying(object sender, EventArgs e)
        {
            if(isMusicWidgetPresent==false) //Avoid unnecessary calls
            RunOnUiThread(() =>
            {
                StartMusicController();

            });
        }


        private void Wallpaper_NewWallpaperIssued(object sender, WallpaperChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                wallpaperView.StartAnimation(fadeoutanimation);

                if (e.Wallpaper == null)
                {
                    wallpaperView.SetBackgroundColor(Color.Black);
                }
                else
                {
                    wallpaperView.Background= e.Wallpaper;
                    wallpaperView.Background.Alpha = e.OpacityLevel;
                }
            });
            GC.Collect();
        }

        private void Lockscreen_Touch(object sender, View.TouchEventArgs e)
        {
            using (ConfigurationManager configuration = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context)))
            {
                if (configuration.RetrieveAValue(ConfigurationParameters.DoubleTapToSleep) == true)
                {
                    if (e.Event.Action == MotionEventActions.Up)
                    {
                        if (firstTouchTime == -1)
                        {
                            firstTouchTime = e.Event.DownTime;
                        }
                        else if (firstTouchTime != -1)
                        {
                            finalTouchTime = e.Event.DownTime;
                            if (firstTouchTime + threshold > finalTouchTime)
                            {
                                //0 Equals: Normal Behavior
                                if (doubletapbehavior=="0")
                                {
                                    if (e.Event.RawY < halfscreenheight)
                                    {
                                        Awake.TurnOffScreen();
                                    }
                                    else
                                    {
                                        //Finish();
                                        //using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(TransparentActivity))))
                                        //{
                                        //    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.);
                                        //    StartActivity(intent);
                                        //}
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
                                        MoveTaskToBack(true);


                                    }
                                    else
                                    {
                                        Awake.TurnOffScreen();
                                    }
                                }

                            }
                            //Reset the values of touch
                            firstTouchTime = -1;
                            finalTouchTime = -1;
                        }
                    }
                }
            }
        }

        private void CheckNotificationListSize()
        {
            if (CatcherHelper.thereAreNotifications == true)
            {
                clearAll.Visibility = ViewStates.Visible;
            }
            else
            {
                clearAll.Visibility = ViewStates.Invisible;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            AddFlags();
            watchDog.Stop();
            watchDog.Start();
            OnActivityStateChanged?.Invoke(this, new LockScreenLifecycleEventArgs { State = ActivityStates.Resumed });
            MusicController.MusicPlaying += MusicController_MusicPlaying;
            MusicController.MusicPaused += MusicController_MusicPaused;

        }

        protected override void OnPause()
        {
            watchDog.Stop();
            OnActivityStateChanged?.Invoke(this, new LockScreenLifecycleEventArgs { State = ActivityStates.Paused });
            MusicController.MusicPlaying -= MusicController_MusicPlaying;
            MusicController.MusicPaused -= MusicController_MusicPaused;
            GC.Collect();
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnActivityStateChanged?.Invoke(this, new LockScreenLifecycleEventArgs { State = ActivityStates.Destroyed });

            //Unbind events
            //unlocker.Touch -= Unlocker_Touch;
            clearAll.Click -= BtnClearAll_Click;
            WallpaperPublisher.NewWallpaperIssued -= Wallpaper_NewWallpaperIssued;
            CatcherHelper.NotificationListSizeChanged -= CatcherHelper_NotificationListSizeChanged;
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
            wallpaperView.Background?.Dispose();
            wallpaperView = null;

            //Dispose Fragments
            notificationFragment.Dispose();
            musicFragment.Dispose();
            clockFragment.Dispose();
            weatherFragment.Dispose();

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
                        clearAll.Visibility = ViewStates.Invisible;
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

        private void Unlocker_Touch(object sender, View.TouchEventArgs e)
        {
            //Log.Info("LiveDisplay", "Y pos:" + lockscreen.GetY());
            //lockscreen.SetY(lockscreen.GetY()-25);

            //lockscreen.SetY(lockscreen.GetY() - e.Event.RawY);

            float startPoint = 0;
            float finalPoint = 0;
            if (e.Event.Action == MotionEventActions.Down)
            {
                Log.Info("Down", e.Event.GetY().ToString());
                startPoint = e.Event.GetY();
            }
            if (e.Event.Action == MotionEventActions.Up)
            {
                Log.Info("Up", e.Event.GetY().ToString());
                finalPoint = e.Event.GetY();
            }
            if (startPoint > finalPoint && finalPoint < 0)
            {
                Log.Info("Swipe", "Up");

                Finish();
                //using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(TransparentActivity))))
                //{
                //    intent.AddFlags(ActivityFlags.NewTask| ActivityFlags.TaskOnHome);
                //    StartActivity(intent);
                //}
                OverridePendingTransition(Resource.Animation.slidetounlockanim, Resource.Animation.slidetounlockanim);
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
                StartActivity(intent);
            }
        }

        private void LoadConfiguration()
        {
            //Load configurations based on User configs.
            using (ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context)))
            {
                switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
                {
                    case "0":

                        WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = null });
                        break;

                    case "1":
                        bool hasReadStoragePermission = true;
                        if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1) //In Lollipop and less this permission is granted at Install time.
                        {
                            hasReadStoragePermission = Checkers.ThisAppHasReadStoragePermission();
                        }
                        if (hasReadStoragePermission)
                            using (var wallpaper = (BitmapDrawable)WallpaperManager.GetInstance(Application.Context).Drawable)
                            {
                                int savedblurlevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, 1);
                                int savedOpacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);
                                BlurImage blurImage = new BlurImage(Application.Context);
                                if (savedblurlevel > 0)
                                {
                                    blurImage.Load(wallpaper.Bitmap).Intensity(savedblurlevel).Async(true);
                                    var blurredwallpaper = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = blurredwallpaper, OpacityLevel = (short)savedOpacitylevel });
                                }
                                else
                                {
                                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = wallpaper, OpacityLevel = (short)savedOpacitylevel });
                                }
                                blurImage = null;
                            }
                        else
                        {
                            RunOnUiThread(() => Toast.MakeText(Application.Context, "You have setted the system wallpaper, but the app can't read it, try to change the Wallpaper option again", ToastLength.Long).Show());
                        }
                        break;

                    case "2":
                        //using (Bitmap bm = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.imagePath, "")))
                        //{
                        //    using (var backgroundFactory = new BackgroundFactory())
                        //    {
                        //        using (BackgroundFactory blurImage = new BackgroundFactory())
                        //        {
                        //            var drawable = blurImage.Difuminar(bm, sa);
                        //            RunOnUiThread(() =>
                        //            Window.DecorView.Background = drawable);
                        //            drawable.Dispose();
                        //        }
                        //    }
                        //}
                        break;

                    default:
                        Window.DecorView.SetBackgroundColor(Color.Black);
                        break;
                }
                if (configurationManager.RetrieveAValue(ConfigurationParameters.MusicWidgetEnabled) == true)
                {
                    CheckIfMusicIsPlaying(); //This method is the main entry for the music widget and the floating notification.
                }
                int interval = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.TurnOffScreenDelayTime, "5000"));
                watchDog.Interval = interval;
                if (configurationManager.RetrieveAValue(ConfigurationParameters.EnableAwakeService) == true)
                {
                    StartAwakeService();
                }
                doubletapbehavior = configurationManager.RetrieveAValue(ConfigurationParameters.DoubleTapOnTopActionBehavior, "0");
            }
        }

        private void LoadNotificationFragment()
        {
            using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
            {
                fragmentTransaction.Replace(Resource.Id.MusicNotificationPlaceholder, notificationFragment);
                fragmentTransaction.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
                fragmentTransaction.DisallowAddToBackStack();
                fragmentTransaction.Commit();
            }
        }

        private void LoadClockFragment()
        {
            using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
            {
                fragmentTransaction.Replace(Resource.Id.weatherandcLockplaceholder, clockFragment);
                fragmentTransaction.DisallowAddToBackStack();
                fragmentTransaction.Commit();
            }
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

        private void CheckIfMusicIsPlaying()
        {
            if (MusicController.MusicStatus == PlaybackStateCode.Playing || MusicControllerKitkat.MusicStatus == RemoteControlPlayState.Playing)
            {
                StartMusicController();
                StartFloatingNotificationService();
                //using (var surfaceView = FindViewById<LinearLayout>(Resource.Id.surfaceview))
                //{
                //    surfaceView.Visibility = ViewStates.Visible;
                //    using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
                //    {
                //        fragmentTransaction.Replace(Resource.Id.FloatingNotificationPlaceholder, new NotificationFragment());
                //        fragmentTransaction.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
                //        fragmentTransaction.DisallowAddToBackStack();
                //        fragmentTransaction.Commit();
                //        NotificationFragment.NotificationClicked += NotificationFragment_NotificationClicked;
                //    }
                //}
            }
            else
            {
                StopMusicController();
                StopFloatingNotificationService();
                //using (var surfaceView = FindViewById<LinearLayout>(Resource.Id.surfaceview))
                //{
                //    surfaceView.Visibility = ViewStates.Gone;
                //}
                //NotificationFragment.NotificationClicked-= NotificationFragment_NotificationClicked;
            }
        }

        private void NotificationFragment_NotificationClicked(object sender, EventArgs e)
        {
            using (var surfaceView = FindViewById<LinearLayout>(Resource.Id.surfaceview))
            {
                if (surfaceView.Visibility != ViewStates.Gone)
                {
                    surfaceView.Visibility = ViewStates.Gone;
                }
                else
                {
                    surfaceView.Visibility = ViewStates.Visible;
                }
            }
        }

        private void StopFloatingNotificationService()
        {
            using (Intent intent = new Intent(Application.Context, typeof(FloatingNotification)))
            {
                StopService(intent);
            }
        }

        private void StartFloatingNotificationService()
        {
            using (Intent intent = new Intent(Application.Context, typeof(FloatingNotification)))
            {
                StartService(intent);
            }
        }

        private void StartAwakeService()
        {
            using (Intent intent = new Intent(Application.Context, typeof(Awake)))
            {
                StartService(intent);
            }
        }

        private void StopAwakeService()
        {
            using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(Awake))))
            {
                StopService(intent);
            }
        }

        
        private void StartMusicController()
        {
            using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
            {
                fragmentTransaction.Replace(Resource.Id.MusicNotificationPlaceholder, musicFragment);
                fragmentTransaction.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
                fragmentTransaction.DisallowAddToBackStack();
                fragmentTransaction.Commit();
            }
            isMusicWidgetPresent = true;
            StartFloatingNotificationService();
        }

        private void StopMusicController()
        {
            LoadNotificationFragment();
            isMusicWidgetPresent = false;
        }
    }
}