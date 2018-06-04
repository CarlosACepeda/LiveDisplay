using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class LockScreenActivity : Activity
    {
        public static LockScreenActivity lockScreenInstance;
        private WallpaperManager wallpaperManager = null;
        private Drawable papelTapiz;
        private LinearLayout linearLayout;
        private View v;
        private RecyclerView recycler;
        private RecyclerView.LayoutManager layoutManager;
        private BackgroundFactory backgroundFactory;
        private TextView tvTitulo;
        private TextView tvTexto;
        private string dia, mes = null;
        private Calendar fecha;
        private TextView tvFecha;
        private LinearLayout reloj;
        private int position;
        private ImageView unlocker;
        private Button btnClearAll;
        //Tiny field used to indicate if Flag.ScreenTurnOn should be applied. (Usually when a new notification is posted)
        //(BEcause Intent for some reason is not Working) by default is false;
        public static bool turnScreenOn = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            BindViews();
            ThreadPool.QueueUserWorkItem(o=>
            InicializarValores());
            //<Start is sensible to configuration LockScreen Settings>
            ThreadPool.QueueUserWorkItem(o => LoadConfiguration());
        }

        protected override void OnResume()
        {
            base.OnResume();     
            BindEvents();
            AddFlags();

            

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
        //OnNotificationItemClick...
        public void OnItemClick(int position)
        {

            //notificationAction = Acciones.RetrieveNotificationAction(position);
            LinearLayout linearLayout = FindViewById<LinearLayout>(Resource.Id.notificationActions);

            OpenNotification notification = new OpenNotification(position);
            
                tvTitulo.Text = notification.GetTitle();
                tvTexto.Text = notification.GetText();
            

                linearLayout.RemoveAllViews();
            linearLayout.WeightSum = 1f;

            if (OpenNotification.RetrieveActionButtons(position) != null)
            {
                foreach (var a in OpenNotification.RetrieveActionButtons(position))
                {
                    linearLayout.AddView(a);
                }
            }
            //actualizar.
            this.position = position;
            v.Visibility = ViewStates.Visible;
            notification = null;
        }
        public void OnItemLongClick(int position)
        {

                 NotificationSlave slave = NotificationSlave.NotificationSlaveInstance();
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    int notiId = CatcherHelper.statusBarNotifications[position].Id;
                    string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
                    string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
                    slave.CancelNotification(notiPack, notiTag, notiId);
                    v.Visibility = ViewStates.Invisible;
                }
                else
                {
                    slave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
                    v.Visibility = ViewStates.Invisible;
                }
            
        }
        private void OnNotificationClicked(object sender, EventArgs e)
        {
            try
            {
                OpenNotification.ClickNotification(position);
            }
            catch
            {
                Log.Wtf("OnNotificationClicked", "Metodo falla porque no existe una notificacion con esta acción");
            }
            v.Visibility = ViewStates.Invisible;
        }
        //Replace with an Event in Acciones.
        //Deprecated.
        public void OnNotificationUpdated()
        {
            OnItemClick(position);
        }
        private void BindViews()
        {
            
            recycler = FindViewById<RecyclerView>(Resource.Id.NotificationListRecyclerView);
            tvTitulo = (TextView)FindViewById(Resource.Id.tvTitulo);
            tvTexto = (TextView)FindViewById(Resource.Id.tvTexto);
            v = FindViewById<View>(Resource.Id.fragment1);
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
            v.Click += OnNotificationClicked;
            reloj.Click += Reloj_Click;           
            btnClearAll.Click += BtnClearAll_Click;
            //TouchEvents
            unlocker.Touch += Unlocker_Touch;
            //If lockscreen no notifications is enabled.
            try
            {
                //Here it goes CatcherHelper Events
            }
            catch {
            }
            
        }



        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance();
            notificationSlave.CancelAll();
            v.Visibility = ViewStates.Invisible;
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
            v.Click -= OnNotificationClicked;
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
            
            
            

            //DodgeNavigationBar();
            
        }       
        private void DodgeNavigationBar()
        {
            //Deprecated.
            int idNavigationBar=
                Resources.GetIdentifier("config_showNavigationBar", "bool", "android");
            bool hasNavigationBar =
                Resources.GetBoolean(idNavigationBar);
            int idNavigationBarHeight=
            Resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            if (idNavigationBarHeight > 0 && hasNavigationBar==true)
            {
                RunOnUiThread(()=>unlocker.SetPadding(0, 0, 0, Resources.GetDimensionPixelSize(idNavigationBarHeight)));
            }
        }
        private void LoadConfiguration()
        {
            //Load configurations based on User configs.
            ConfigurationManager configurationManager = new ConfigurationManager(GetSharedPreferences("livedisplayconfig", FileCreationMode.Private));
            if (configurationManager.RetrieveAValue(ConfigurationParameters.enabledLockscreen) == true)
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
                if (configurationManager.RetrieveAValue(ConfigurationParameters.enabledlockscreennonotifications) == false)
                {
                    //Finish this activity because user disabled this option Should never happen normally.
                    RunOnUiThread(() => Finish());                    
                }               
                if (configurationManager.RetrieveAValue(ConfigurationParameters.dynamicwallpaperenabled) == true)
                {
                    //Allow the app to show Album art.
                }
                if (String.Equals(configurationManager.RetrieveAValue(ConfigurationParameters.imagePath, ""), "imagenotfound") == false)
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
                    wallpaperManager = WallpaperManager.GetInstance(this);
                    papelTapiz = wallpaperManager.Drawable;
                    backgroundFactory = new BackgroundFactory();
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        Drawable background = backgroundFactory.Difuminar(papelTapiz);

                            RunOnUiThread(() => Window.DecorView.Background = background);
                       
                    });
                }
            }
            else //SHould never happen normally.
            {
                Finish();
            }
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
    }

}