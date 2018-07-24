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
        private Calendar calendar;
        private TextView tvFecha;
        private LinearLayout reloj;
        private ImageView unlocker;
        private Button btnClearAll;
        private NotificationFragment notificationFragment;
        private MusicFragment musicFragment;

        public event EventHandler<NotificationItemClickedEventArgs> NotificationItemClicked;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Console.Write("OnCreate called");
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            //Views
            
            tvFecha = FindViewById<TextView>(Resource.Id.txtFechaLock);
            reloj = FindViewById<LinearLayout>(Resource.Id.clockLock);
            unlocker = FindViewById<ImageView>(Resource.Id.unlocker);
            btnClearAll = FindViewById<Button>(Resource.Id.btnClearAllNotifications);
            notificationFragment = new NotificationFragment();
            musicFragment = new MusicFragment();

            //View Events
            reloj.Click += Reloj_Click;
            btnClearAll.Click += BtnClearAll_Click;
            unlocker.Touch += Unlocker_Touch;

            //Media Controller events
            ActiveMediaSessionsListener.MediaSessionStarted += MusicController_MediaSessionStarted;
            ActiveMediaSessionsListener.MediaSessionStopped += MusicController_MediaSessionStopped;

            

            //Load date
            using (calendar = Calendar.GetInstance(Locale.Default))
            {
                tvFecha.Text = string.Format(calendar.Get(CalendarField.DayOfMonth).ToString() + ", " + calendar.GetDisplayName((int)CalendarField.Month, (int)CalendarStyle.Long, Locale.Default));
            }

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
            //Load User Configs.
            ThreadPool.QueueUserWorkItem(o => LoadConfiguration());
            
            LoadNotificationFragment();
            CheckIfMusicIsPlaying();
            
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
            reloj.Click -= Reloj_Click;
            unlocker.Touch -= Unlocker_Touch;
            btnClearAll.Click -= BtnClearAll_Click;
            ActiveMediaSessionsListener.MediaSessionStarted -= MusicController_MediaSessionStarted;
            ActiveMediaSessionsListener.MediaSessionStopped -= MusicController_MediaSessionStopped;

            //Dispose Views
            //Views
            recycler.Dispose();
            tvFecha.Dispose();
            reloj.Dispose();
            unlocker.Dispose();
            btnClearAll.Dispose();

            //Dispose Fragments
            notificationFragment.Dispose();
            musicFragment.Dispose();


        }

        public override void OnBackPressed()
        {
            //Do nothing.
            //base.OnBackPressed();
        }

        //Los siguientes 2 métodos son Callbacks desde el Adaptador del RecyclerView
        public void OnItemClick(int position)
        {
            
            //TODO: Dont call this if the Music is playing
            OnNotificationItemClicked(position);
            //Instead, show another view.
            
        }

        private void OnNotificationItemClicked(int position)
        {
            NotificationItemClicked?.Invoke(this, new NotificationItemClickedEventArgs
            {
                Position = position
            });
        }

        public void OnItemLongClick(int position)
        {
            //TODO: When this is called communicate with Notification Widget
            //and tell it to Unload the data and make itself invisible
            using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    int notiId = CatcherHelper.statusBarNotifications[position].Id;
                    string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
                    string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
                    slave.CancelNotification(notiPack, notiTag, notiId);
                    //TODO: Tell the Fragment to unload the data instead
                    //v.Visibility = ViewStates.Invisible;
                }
                else
                {
                    slave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
                    //TODO: Tell the Fragment to unload the data instead
                    //v.Visibility = ViewStates.Invisible;

                }
            }
            

        }
       
        //Deprecated.
        public void OnNotificationUpdated()
        {
            //TODO: Make a Event listener to Update the information instead of Clicking the Item again
        }


        private void MusicController_MediaSessionStarted(object sender, EventArgs e)
        {
            StartMusicController();
            if (recycler != null)
            {
                recycler.Visibility = ViewStates.Invisible;
            }
        }
        private void MusicController_MediaSessionStopped(object sender, EventArgs e)
        {
            StopMusicController();
            if (recycler != null)
            {
                recycler.Visibility = ViewStates.Visible;
            }
            //TODO: Once Stopped, set the background image again.
        }
       
        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            using (NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance())
            {
                //TODO: Invoke an event that NotificationFragment will be subscribed to.
                //This method will say: Hey, hide! All the notifications are removed.
                //You don't need to exist anymore
            }
            
            
        }
        private void Reloj_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent(AlarmClock.ActionShowAlarms))
            {
                StartActivity(intent);
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
        
       
        private void LoadConfiguration()
        {
            //Load configurations based on User configs.
            using (ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(this)))
            {
                if (configurationManager.RetrieveAValue(ConfigurationParameters.hiddenclock) == true)
                {
                    //Hide the clock
                    RunOnUiThread(() => reloj.Visibility = ViewStates.Gone);
                }
                if (configurationManager.RetrieveAValue(ConfigurationParameters.hiddensystemicons) == true)
                {
                    //Hide system icons, when available, FIX ME.
                }

                if (configurationManager.RetrieveAValue(ConfigurationParameters.dynamicwallpaperenabled) == true)
                {
                    //Allow the app to show Album art.
                    //:TODO move to Music Fragment, not here.
                }
                if (String.Equals(configurationManager.RetrieveAValue(ConfigurationParameters.imagePath, "imagenotfound"), "imagenotfound") == false)
                {
                    //Found an image, use it as wallpaper.
                    using (Bitmap bm = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.imagePath, "")))
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
                //Also disable the NotificationList to show.
                recycler.Visibility = ViewStates.Invisible;
            }
        }

        /// <summary>
        /// Call this method automatically when Music is playing
        /// </summary>
        private void StartMusicController()
        {
            using (musicFragment = new MusicFragment())
            {
                using (FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction())
                {
                    fragmentTransaction.Replace(Resource.Id.MusicNotificationPlaceholder, musicFragment);
                    fragmentTransaction.DisallowAddToBackStack();
                    fragmentTransaction.Commit();
                }
            }
                
            
           
            
        }
        //When music is stopped, call this.
        private void StopMusicController()
        {
            try
            {
                LoadNotificationFragment();
                
            }
            catch
            {
                
                Console.WriteLine("Failed to Stop Music Controller");
            }
            
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