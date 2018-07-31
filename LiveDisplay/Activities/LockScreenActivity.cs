using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Factories;
using LiveDisplay.Fragments;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", Theme ="@style/LiveDisplayTheme.NoActionBar", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = Android.Content.PM.LaunchMode.SingleTask, ExcludeFromRecents = true)]
    public class LockScreenActivity : Activity
    {
        private RecyclerView recycler;
        private RecyclerView.LayoutManager layoutManager;
        private ImageView unlocker;
        private Button clearAll;
        private NotificationFragment notificationFragment;
        private MusicFragment musicFragment;
        private ClockFragment clockFragment;
        private bool thereAreNotifications = false;
        private Button startCamera;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info("Alert: ","OnCreate called");
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            //Views
            
            
            unlocker = FindViewById<ImageView>(Resource.Id.unlocker);
            startCamera = FindViewById<Button>(Resource.Id.btnStartCamera);
            clearAll = FindViewById<Button>(Resource.Id.btnClearAllNotifications);
            notificationFragment = new NotificationFragment();
            musicFragment = new MusicFragment();
            clockFragment = new ClockFragment();


            
            clearAll.Click += BtnClearAll_Click;
            unlocker.Touch += Unlocker_Touch;
            startCamera.Click += StartCamera_Click;

            //Media Controller events
            ActiveMediaSessionsListener.MediaSessionStarted += MusicController_MediaSessionStarted;
            ActiveMediaSessionsListener.MediaSessionStopped += MusicController_MediaSessionStopped;

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
            
            //Load Default Wallpaper
            LoadDefaultWallpaper();

            //Load cLock Widget
            LoadClockFragment();

            //Load User Configs.
            ThreadPool.QueueUserWorkItem(o => LoadConfiguration());
            
            //Load Notification Fragment
            LoadNotificationFragment();

            //Check if music is playing
            CheckIfMusicIsPlaying();

            //Check the Notification List Size
            CheckNotificationListSize();            
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
            //StartTimerToLockScreen();
            //Add Immersive Flags
            AddFlags();


        }
        protected override void OnPause()
        {
            GC.Collect();
            base.OnPause();

        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            //Unbind events
            unlocker.Touch -= Unlocker_Touch;
            clearAll.Click -= BtnClearAll_Click;
            ActiveMediaSessionsListener.MediaSessionStarted -= MusicController_MediaSessionStarted;
            ActiveMediaSessionsListener.MediaSessionStopped -= MusicController_MediaSessionStopped;
            CatcherHelper.NotificationListSizeChanged -= CatcherHelper_NotificationListSizeChanged;

            //Dispose Views
            //Views
            recycler.Dispose();
            unlocker.Dispose();
            clearAll.Dispose();

            //Dispose Fragments
            notificationFragment.Dispose();
            musicFragment.Dispose();
            clockFragment.Dispose();


        }
        public override void OnBackPressed()
        {
            //Do nothing.
            //base.OnBackPressed();
        }
        private void MusicController_MediaSessionStarted(object sender, EventArgs e)
        {
            StartMusicController();
        }
        private void MusicController_MediaSessionStopped(object sender, EventArgs e)
        {
            StopMusicController();
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
                //TODO: Invoke an event that NotificationFragment will be subscribed to.
                //This method will say: Hey, hide! All the notifications are removed.
                //You don't need to exist anymore
                notificationSlave.CancelAll();

            }         
        }
        private void Unlocker_Touch(object sender, View.TouchEventArgs e)
        {
            //TODO: Document me
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
                finalPoint= e.Event.GetY();
            }
            if (startPoint > finalPoint && finalPoint<0)
            {
                Log.Info("Swipe", "Up");

                Finish();
                OverridePendingTransition(Resource.Animation.slidetounlockanim, Resource.Animation.slidetounlockanim);
            }

        }

        //switch (e.Event.Action)
        //{
        //    case MotionEventActions.Down:
        //        x = linearLayout.GetX()- e.Event.RawX;
        //        break;
        //    case MotionEventActions.Move:
        //        linearLayout.Animate().X(e.Event.RawX + x)
        //            .SetDuration(0)
        //            .Start();

        //        break;

        //}
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
            using (ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this))
            {               
                if (sharedPreferences.GetBoolean(ConfigurationParameters.hiddensystemicons,false) == true)
                {
                    //Hide system icons, when available, FIX ME.
                }

                if (sharedPreferences.GetBoolean(ConfigurationParameters.dynamicwallpaperenabled, false) == true)
                {
                    //Allow the app to show Album art.
                    //:TODO move to Music Fragment, not here.
                }
                if (String.Equals(sharedPreferences.GetString(ConfigurationParameters.imagePath, "imagenotfound"), "imagenotfound") == false)
                {
                    //Found an image, use it as wallpaper.
                    using (Bitmap bm = BitmapFactory.DecodeFile(sharedPreferences.GetString(ConfigurationParameters.imagePath, "")))
                    {
                        using (var backgroundFactory = new BackgroundFactory())
                        {
                            using (BackgroundFactory blurImage = new BackgroundFactory())
                            {
                                ThreadPool.QueueUserWorkItem(m =>
                                {
                                    var drawable = blurImage.Difuminar(bm);
                                    RunOnUiThread(() =>
                                    Window.DecorView.Background = drawable);
                                    drawable.Dispose();
                                }
                                );

                            }

                        }
                    }

                }
            }
                
        }
        private void LoadNotificationFragment()
        {

            using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
            {

                fragmentTransaction.Replace(Resource.Id.MusicNotificationPlaceholder, notificationFragment);
                fragmentTransaction.DisallowAddToBackStack();
                fragmentTransaction.Commit();
            }
        }

        private void LoadClockFragment()
        {
            using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
            {

                fragmentTransaction.Replace(Resource.Id.cLockPlaceholder, clockFragment);
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
        /// <summary>
        /// This method calls Awake#LockScreen
        /// </summary>
        private void StartTimerToLockScreen()
        {
            Awake.LockScreen();
        }
        //TODO:
        //If Lockscreen register a click, reset the timer.

        private void CheckIfMusicIsPlaying()
        {
            if (ActiveMediaSessionsListener.IsASessionActive == true)
            {
                StartMusicController();
            }
        }

        /// <summary>
        /// Call this method automatically when Music is playing
        /// </summary>
        private void StartMusicController()
        {
                using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
                {
                    fragmentTransaction.Replace(Resource.Id.MusicNotificationPlaceholder, musicFragment);
                    fragmentTransaction.DisallowAddToBackStack();
                    fragmentTransaction.Commit();
                }
            
                
        }
        //When music is stopped, call this.
        private void StopMusicController()
        {
            LoadNotificationFragment();                         
        }

        private void LoadDefaultWallpaper()
        {
            using (BackgroundFactory blurImage = new BackgroundFactory())
            {
                using (WallpaperManager wallpaperManager = WallpaperManager.GetInstance(Application.Context))
                {
                    wallpaperManager.ForgetLoadedWallpaper();//Forget the loaded wallpaper because it will be the one that is blurred.
                                                             //so, it will blur the already blurred wallpaper, so, causing so much blur that 
                                                         //the app will explode, LOL.
                    using (BitmapDrawable bitmap = (BitmapDrawable)wallpaperManager.Drawable)
                    {
                            var drawable = blurImage.Difuminar(bitmap.Bitmap);
                            RunOnUiThread(() =>
                            Window.DecorView.Background = drawable);
                            drawable.Dispose();

                    }
                }

            }
            }
            
        

    }

}