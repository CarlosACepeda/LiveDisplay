using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Factories;
using LiveDisplay.Fragments;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Threading;
using Android.Media.Session;
using LiveDisplay.Servicios.FloatingNotification;
using Com.JackAndPhantom;
using Android.Graphics.Drawables;
using Android.Media;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", Theme = "@style/LiveDisplayThemeDark", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = Android.Content.PM.LaunchMode.SingleTask, ExcludeFromRecents = true)]
    public class LockScreenActivity : Activity, ISensorEventListener
    {
        private RecyclerView recycler;
        private RecyclerView.LayoutManager layoutManager;
        private ImageView wallpaper;
        private ImageView unlocker;
        private Button clearAll;
        private FrameLayout weatherandclockcontainer;
        private NotificationFragment notificationFragment;
        private MusicFragment musicFragment;
        private ClockFragment clockFragment;
        private WeatherFragment weatherFragment;
        private bool thereAreNotifications = false;
        private Button startCamera;
        private LinearLayout lockscreen; //The root linear layout, used to implement double tap to sleep.
        private float firstTouchTime = -1;
        private float finalTouchTime;
        private readonly float threshold = 1000; //1 second of threshold.(used to implement the double tap.)
        private Sensor sensor;
        private SensorManager sensorManager;
        private System.Timers.Timer watchDog; //the watchdog simply will start counting down until it gets resetted by OnUserInteraction() override.



        protected override void OnCreate(Bundle savedInstanceState)
        {
           
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);

            //Before loading anything, check if the user has the required permissions.
            ThreadPool.QueueUserWorkItem(isApphealthy =>
            {
                bool canDrawOverlays = true;
                if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1) //In Lollipop and less this permission is granted at Install time.
                {
                    canDrawOverlays = Checkers.ThisAppCanDrawOverlays();
                }

                if (Checkers.IsNotificationListenerEnabled() == false  || canDrawOverlays == false)
                {
                    RunOnUiThread(()=>
                    Toast.MakeText(Application.Context, "You dont have the required permissions", ToastLength.Long).Show()
                    );
                    Finish();
                }

            });



            //Views
            wallpaper = FindViewById<ImageView>(Resource.Id.wallpaper);
            unlocker = FindViewById<ImageView>(Resource.Id.unlocker);
            startCamera = FindViewById<Button>(Resource.Id.btnStartCamera);
            clearAll = FindViewById<Button>(Resource.Id.btnClearAllNotifications);
            lockscreen = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            weatherandclockcontainer = FindViewById<FrameLayout>(Resource.Id.weatherandcLockplaceholder);
            notificationFragment = new NotificationFragment();
            musicFragment = new MusicFragment();
            clockFragment = new ClockFragment();
            weatherFragment = new WeatherFragment();
            clearAll.Click += BtnClearAll_Click;
            unlocker.Touch += Unlocker_Touch;
            startCamera.Click += StartCamera_Click;
            lockscreen.Touch += Lockscreen_Touch;
            weatherandclockcontainer.LongClick += Weatherandclockcontainer_LongClick;

            watchDog = new System.Timers.Timer();
            watchDog.AutoReset = false;
            watchDog.Interval = 5000;
            watchDog.Elapsed += WatchdogInterval_Elapsed;

            WallpaperPublisher.WallpaperChanged += Wallpaper_WallpaperChanged;

            //Music Controller Events
            if(MusicController.MusicStatus!= PlaybackStateCode.None)
            {
                MusicController.MusicPlaying += MusicController_MusicPlaying;
                MusicController.MusicPaused += MusicController_MusicPaused;
                MusicController.MusicStopped += MusicController_MusicStopped;


            }
            //CatcherHelper events
            CatcherHelper.NotificationListSizeChanged += CatcherHelper_NotificationListSizeChanged;

            //Load RecyclerView

            using (recycler = FindViewById<RecyclerView>(Resource.Id.NotificationListRecyclerView))
            {
                using (layoutManager = new LinearLayoutManager(Application.Context))
                {
                    recycler.SetLayoutManager(layoutManager);
                    recycler.SetAdapter(CatcherHelper.notificationAdapter);
                }
            }


            LoadClockFragment();

            //Load User Configs.
            LoadConfiguration();

            
            LoadNotificationFragment();

            CheckIfMusicIsPlaying();

            
            CheckNotificationListSize();


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
            ThreadPool.QueueUserWorkItem(m => CheckIfMusicIsStillPaused());
        }

        private void CheckIfMusicIsStillPaused()
        {
            Thread.Sleep(5000); //Wait 5 seconds.

            if(MusicController.MusicStatus!= PlaybackStateCode.Playing)
            {
                StopMusicController();
                Log.Info("LiveDisplay", "Musicfragment stopped");
            }
        }

        private void MusicController_MusicPlaying(object sender, EventArgs e)
        {
            StartMusicController();
        }

        private void Weatherandclockcontainer_LongClick(object sender, View.LongClickEventArgs e)
        {
            using (var fragmentTransaction = FragmentManager.BeginTransaction())
            {
                fragmentTransaction.Replace(Resource.Id.weatherandcLockplaceholder, weatherFragment);
                fragmentTransaction.Commit();
            }
        }

        private void Wallpaper_WallpaperChanged(object sender, WallpaperChangedEventArgs e)
        {
            if (e.Wallpaper == null)
            {

                wallpaper.SetBackgroundColor(Color.Black);
            }
            else
            {
                wallpaper.Background = e.Wallpaper;
                wallpaper.Background.Alpha = e.OpacityLevel;
            }
            GC.Collect();

        }

        private void Lockscreen_Touch(object sender, View.TouchEventArgs e)
        {
            using (ConfigurationManager configuration = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context)))
            {
                if (configuration.RetrieveAValue(ConfigurationParameters.doubletaptosleep) == true)
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
                                Awake.TurnOffScreen();
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
        }

        protected override void OnPause()
        {
            MusicController.MusicPlaying -= MusicController_MusicPlaying;
            MusicController.MusicPaused -= MusicController_MusicPaused;
            MusicController.MusicStopped -= MusicController_MusicStopped;
            watchDog.Stop();
            GC.Collect();
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //Unbind events
            unlocker.Touch -= Unlocker_Touch;
            clearAll.Click -= BtnClearAll_Click;
            WallpaperPublisher.WallpaperChanged -= Wallpaper_WallpaperChanged;
            CatcherHelper.NotificationListSizeChanged -= CatcherHelper_NotificationListSizeChanged;
            lockscreen.Touch -= Lockscreen_Touch;

            watchDog.Stop();
            watchDog.Elapsed -= WatchdogInterval_Elapsed;
            watchDog.Dispose();

            //Dispose Views
            //Views
            recycler.Dispose();
            unlocker.Dispose();
            clearAll.Dispose();
            lockscreen.Dispose();
            wallpaper.Background.Dispose();
            wallpaper.Dispose();

            //Dispose Fragments
            notificationFragment.Dispose();
            musicFragment.Dispose();
            clockFragment.Dispose();

            StopFloatingNotificationService();
            
        }

        public override void OnBackPressed()
        {
            //Do nothing.
            //base.OnBackPressed();
            //In Nougat it works after several tries to go back, I can't fix that.
        }

        //It simply means that a Touch has been registered, no matter where, it was on the lockscreen.
        //used to detect if the user is interacting with the lockscreen.
        public override void OnUserInteraction()
        {
            base.OnUserInteraction();
            //Refresh the Watchdog, damn dog, annoying, lol.
            watchDog.Stop();
            watchDog.Start();
            Log.Info("LiveDisplay", "User is touching Lockscreen");
        }


        private void CatcherHelper_NotificationListSizeChanged(object sender, NotificationListSizeChangedEventArgs e)
        {
            if (e.ThereAreNotifications == true)
            {
                try
                {
                    clearAll.Visibility = ViewStates.Visible;
                }
                catch
                {
                    thereAreNotifications = e.ThereAreNotifications;
                }
            }
            else
            {
                try
                {
                    clearAll.Visibility = ViewStates.Invisible;
                }
                catch
                {
                    thereAreNotifications = e.ThereAreNotifications;
                }
            }
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
                OverridePendingTransition(Resource.Animation.slidetounlockanim, Resource.Animation.slidetounlockanim);
            }
        }

        private void StartCamera_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent("android.media.action.IMAGE_CAPTURE"))
            {
                StartActivity(intent);
            }
        }

        private void LoadConfiguration()
        {
            //Load configurations based on User configs.
            using (ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context)))
            {
                switch (configurationManager.RetrieveAValue(ConfigurationParameters.changewallpaper, "0"))
                {
                    case "0":

                        WallpaperPublisher.OnWallpaperChanged(new WallpaperChangedEventArgs { Wallpaper = null });
                        break;

                    case "1":
                            using (var wallpaper = WallpaperManager.GetInstance(Application.Context).Drawable)
                            {
                                int savedblurlevel = configurationManager.RetrieveAValue(ConfigurationParameters.blurlevel, 1);
                                int savedOpacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.opacitylevel, 255);
                                //var weakblur = new WeakReference(new BlurImage(Application.Context).Load(bitmap).Intensity(savedblurlevel).Async(true).GetImageBlur());
                                //var weak = new WeakReference(new BitmapDrawable(Resources, weakblur.Target as Bitmap));

                                
                                 RunOnUiThread(() =>
                                    WallpaperPublisher.OnWallpaperChanged(new WallpaperChangedEventArgs { Wallpaper = wallpaper, OpacityLevel = (short)savedOpacitylevel })
                                          );
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
            }
            else
            {
                StopMusicController();
                StopFloatingNotificationService();
            }
        }

        private void StopFloatingNotificationService()
        {
            using (Intent intent = new Intent(this, typeof(FloatingNotification)))
            {
                StopService(intent);
            }

        }

        private void StartFloatingNotificationService()
        {
            using (Intent intent = new Intent(this, typeof(FloatingNotification)))
            {
                StartService(intent);
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
        }

        private void StopMusicController()
        {
            LoadNotificationFragment();
        }


        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            throw new NotImplementedException();
        }

        public void OnSensorChanged(SensorEvent e)
        {
            throw new NotImplementedException();
        }
    }
}
