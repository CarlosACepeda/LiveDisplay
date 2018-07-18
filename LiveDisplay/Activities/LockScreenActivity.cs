using Android.App;
using Android.Content;
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
    [Activity(Label = "LockScreen", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, ExcludeFromRecents = true)]
    public class LockScreenActivity : Activity
    {
        public static LockScreenActivity lockScreenInstance;
        private WallpaperManager wallpaperManager = null;
        private Drawable papelTapiz;
        private LinearLayout linearLayout;
        private RecyclerView recycler;
        private RecyclerView.LayoutManager layoutManager;
        private BackgroundFactory backgroundFactory;
        private string dia, mes;
        private Calendar fecha;
        private TextView tvFecha;
        private LinearLayout reloj;
        private ImageView unlocker;
        private Button btnClearAll;
        private Fragment notificationFragment;
        public event EventHandler<NotificationItemClickedEventArgs> NotificationItemClicked;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            BindViews();
            BindMediaControllerEvents();
            ThreadPool.QueueUserWorkItem(o =>
            InicializarValores());
            ThreadPool.QueueUserWorkItem(o => LoadConfiguration());
        }
        protected override void OnResume()
        {
            base.OnResume();
            BindEvents();
            AddFlags();
            StartTimerToLockScreen();
            CheckIfMusicIsPlaying();
        }
        protected override void OnPause()
        {
            base.OnPause();
            UnbindEvents();

        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnbindViews();
            GC.Collect();
        }

        //Los siguientes 2 métodos son Callbacks desde el Adaptador del RecyclerView
        public void OnItemClick(int position)
        {
            
            //TODO: Dont call this if the Music is playing
            OnNotificationItemClicked(position);
            
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
            NotificationSlave slave = NotificationSlave.NotificationSlaveInstance();
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
       
        //Deprecated.
        public void OnNotificationUpdated()
        {
            //TODO: Make a Event listener to Update the information instead of Clicking the Item again
        }
        private void BindViews()
        {

            recycler = FindViewById<RecyclerView>(Resource.Id.NotificationListRecyclerView);            
            tvFecha = (TextView)FindViewById(Resource.Id.txtFechaLock);
            reloj = FindViewById<LinearLayout>(Resource.Id.clockLock);
            unlocker = FindViewById<ImageView>(Resource.Id.unlocker);
            btnClearAll = FindViewById<Button>(Resource.Id.btnClearAllNotifications);
            linearLayout = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
        }      
        private void UnbindViews()
        {
            linearLayout = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            for (int i = 0; i < linearLayout.ChildCount; i++)
            {
                View view = linearLayout.GetChildAt(i);
                view.Dispose();
                view = null;
            }
            lockScreenInstance.Dispose();
            lockScreenInstance = null;
        }
        private void BindEvents()
        {
            //Click events
            reloj.Click += Reloj_Click;
            btnClearAll.Click += BtnClearAll_Click;
            //TouchEvents
            unlocker.Touch += Unlocker_Touch;
            

        }
        private void BindMediaControllerEvents()
        {
            ActiveMediaSessionsListener.MediaSessionStarted += MusicController_MediaSessionStarted;
            ActiveMediaSessionsListener.MediaSessionStopped += MusicController_MediaSessionStopped;
            Console.WriteLine("Eventos enlazados");
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
            NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance();
            FragmentTransaction fragmentManager = FragmentManager.BeginTransaction();
            fragmentManager.Remove(notificationFragment);
            fragmentManager.Commit();
            fragmentManager.Dispose();
            notificationSlave = null;
        }
        private void Reloj_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(AlarmClock.ActionShowAlarms);
            StartActivity(intent);
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
        
        private void UnbindEvents()
        {
            reloj.Click -= Reloj_Click;
            unlocker.Touch -= Unlocker_Touch;
            btnClearAll.Click -= BtnClearAll_Click;
            
        }
        private void InicializarValores()
        {
            lockScreenInstance = this;
            layoutManager = new LinearLayoutManager(this);
            
            fecha = Calendar.GetInstance(Locale.Default);
            dia = fecha.Get(CalendarField.DayOfMonth).ToString();
            mes = fecha.GetDisplayName(2, 2, Locale.Default);
            RunOnUiThread(() =>
            {
                AddFlags();
                recycler.SetLayoutManager(layoutManager);
                recycler.SetAdapter(CatcherHelper.notificationAdapter);
                tvFecha.Text = string.Format(dia + ", " + mes);
            });
            
            
        }       
       
        private void LoadConfiguration()
        {
            //Load configurations based on User configs.
            ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(this));
                if (configurationManager.RetrieveAValue(ConfigurationParameters.hiddenclock)==true)
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
                    Bitmap bm = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.imagePath, ""));

                        backgroundFactory = new BackgroundFactory();
                        Drawable background = backgroundFactory.Difuminar(bm);
                        RunOnUiThread(() => Window.DecorView.Background = background);
                        backgroundFactory = null;
                        bm = null;
                    
                }
                else
                {
                LoadDefaultWallpaper();                  
                }
        }
        private void LoadNotificationFragment()
        {
            notificationFragment = new NotificationFragment();
            FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.MusicNotificationPlaceholder, notificationFragment);
            fragmentTransaction.DisallowAddToBackStack();
            fragmentTransaction.Commit();
        }


        private void AddFlags()
        {
           View view = Window.DecorView;
            var uiOptions = (int)view.SystemUiVisibility;
            var newUiOptions = uiOptions;

            newUiOptions |= (int)SystemUiFlags.LowProfile;
            newUiOptions |= (int)SystemUiFlags.Fullscreen;
            newUiOptions |= (int)SystemUiFlags.HideNavigation;
            newUiOptions |= (int)SystemUiFlags.Immersive;
            // This option will make bars disappear by themselves
            newUiOptions |= (int)SystemUiFlags.ImmersiveSticky;

            view.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
            Window.AddFlags(WindowManagerFlags.DismissKeyguard);
            Window.AddFlags(WindowManagerFlags.ShowWhenLocked);
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
            if (ActiveMediaSessionsListener.isASessionActive == true)
            {
                StartMusicController();
                //Also disable the NotificationList to show.
                recycler.Visibility = ViewStates.Invisible;
            }
            else if (ActiveMediaSessionsListener.isASessionActive == false)
            {

                StopMusicController();
                              
                recycler.Visibility = ViewStates.Visible;
            }
        }

        /// <summary>
        /// Call this method automatically when Music is playing
        /// </summary>
        private void StartMusicController()
        {
            
            try
            {
            Fragment fragment = new MusicFragment();
            FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.MusicNotificationPlaceholder, fragment);
            fragmentTransaction.DisallowAddToBackStack();
            fragmentTransaction.Commit();
            }
            catch
            {
                Console.WriteLine("Failed to start music widget");
            }
            
        }
        //When music is stopped, call this.
        private void StopMusicController()
        {
            try
            {
                LoadNotificationFragment();
                LoadDefaultWallpaper();
            }
            catch
            {
                
                Console.WriteLine("Failed to Stop Music Controller");
            }
            
        }

        private void LoadDefaultWallpaper()
        {
            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;
            BitmapDrawable bitmap = (BitmapDrawable)papelTapiz;

            Com.JackAndPhantom.BlurImage blurImage = new Com.JackAndPhantom.BlurImage(this);
            blurImage.Load(bitmap.Bitmap);
            blurImage.Intensity(25);


            BitmapDrawable drawable = new BitmapDrawable(blurImage.GetImageBlur());

            RunOnUiThread(() => Window.DecorView.Background = drawable);

        }
    }

}