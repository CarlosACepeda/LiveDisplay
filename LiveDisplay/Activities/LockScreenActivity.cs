using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.Factories;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Notificaciones;
using System;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class LockScreenActivity : Activity
    {
        private int oldListSize, newListSize = 0;
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
        private PendingIntent notificationAction;
        private string dia, mes = null;
        private Calendar fecha;
        private TextView tvFecha;
        private TextClock reloj;
        private int position;
        private LinearLayout unlocker;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            
        }

        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                newListSize = Catcher.listaNotificaciones.Count;
            }
            catch
            {
                Toast.MakeText(ApplicationContext, "¡No puedo mostrar notificaciones, el servicio debe estar Activado usando el menu de ajustes de esta app", ToastLength.Long).Show();
            }
            BindViews();
            BindClickEvents();
            InicializarValores();
            CheckDataChanges();
        }

        protected override void OnPause()
        {
            base.OnPause();
            try
            {
                oldListSize = Catcher.listaNotificaciones.Count;
            }
            catch
            {
            };
            UnbindClickEvents();
            UnbindViews();
            GC.Collect();
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            GC.Collect();
        }

        //Los siguientes 2 métodos son Callbacks desde el Adaptador del RecyclerView
        //OnNotificationItemClick...
        public void OnItemClick(int position)
        {
            notificationAction = Acciones.RetrieveNotificationAction(position);
            LinearLayout linearLayout = FindViewById<LinearLayout>(Resource.Id.notificationActions);
            var newType = Contents.RetrieveNotificationContents(position);

            tvTitulo.Text = newType != null ? newType.Item2 : "";
            tvTexto.Text = newType != null ? newType.Item3 : "";
            linearLayout.RemoveAllViews();

            if (Acciones.RetrieveNotificationButtonsActions(position, newType.Item1) != null)
            {
                foreach (var a in Acciones.RetrieveNotificationButtonsActions(position, newType.Item1))
                {
                    linearLayout.AddView(a);
                }
            }
            //actualizar.
            this.position = position;
            v.Visibility = ViewStates.Visible;
        }

        public void OnItemLongClick(int position)
        {
            if (Catcher.listaNotificaciones[position].IsClearable == true)
            {
                NotificationSlave slave = new NotificationSlave();
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    int notiId = Catcher.listaNotificaciones[position].Id;
                    string notiTag = Catcher.listaNotificaciones[position].Tag;
                    string notiPack = Catcher.listaNotificaciones[position].PackageName;
                    slave.CancelNotification(notiPack, notiTag, notiId);
                    v.Visibility = ViewStates.Invisible;
                }
                else
                {
                    slave.CancelNotification(Catcher.listaNotificaciones[position].Key);
                    v.Visibility = ViewStates.Invisible;
                }
            }
            else
            {
                v.SetBackgroundColor(Android.Graphics.Color.Red);
            }
        }

        //In Catcher.
        private void CheckDataChanges()
        {
            if (oldListSize != newListSize)
            {
                Catcher.adapter.NotifyDataSetChanged();
            }
        }

        private void OnNotificationClicked(object sender, EventArgs e)
        {
            try
            {
                notificationAction.Send();
            }
            catch
            {
                Log.Wtf("OnNotificationClicked", "Metodo falla porque no existe una notificacion con esta acción");
            }
            v.Visibility = ViewStates.Invisible;
        }

        public void OnNotificationUpdated()
        {
            OnItemClick(position);
        }

        private void BindViews()
        {
            linearLayout = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            recycler = FindViewById<RecyclerView>(Resource.Id.NotificationListRecyclerView);
            tvTitulo = (TextView)FindViewById(Resource.Id.tvTitulo);
            tvTexto = (TextView)FindViewById(Resource.Id.tvTexto);
            v = FindViewById<View>(Resource.Id.fragment1);
            tvFecha = (TextView)FindViewById(Resource.Id.txtFechaLock);
            reloj = FindViewById<TextClock>(Resource.Id.clockLock);
            unlocker = FindViewById<LinearLayout>(Resource.Id.unlocker);
        }

        private void UnbindViews()
        {
            linearLayout = null;
            recycler = null;
            tvTitulo = null;
            tvTexto = null;
            v = null;
            wallpaperManager = null;
            papelTapiz = null;
            layoutManager = null;
            lockScreenInstance = null;
            backgroundFactory = null;
            fecha = null;
            dia = null;
            mes = null;
            notificationAction = null;
            reloj = null;
        }

        private void BindClickEvents()
        {
            v.Click += OnNotificationClicked;
            reloj.Click += Reloj_Click;
            unlocker.Touch += Unlocker_Touch;
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


        private void Reloj_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(AlarmClock.ActionShowAlarms);
            StartActivity(intent);
        }

        private void UnbindClickEvents()
        {
            v.Click -= OnNotificationClicked;
            reloj.Click -= Reloj_Click;
        }

        private void InicializarValores()
        {
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;
            backgroundFactory = new BackgroundFactory();
            ThreadPool.QueueUserWorkItem(o =>
            {
               Drawable background= backgroundFactory.Difuminar(papelTapiz);

                RunOnUiThread(() => linearLayout.Background = background);
                
            });
            
            layoutManager = new LinearLayoutManager(this);
            recycler.SetLayoutManager(layoutManager);
            RunOnUiThread(() => recycler.SetAdapter(Catcher.adapter));
            fecha = Calendar.GetInstance(Locale.Default);
            dia = fecha.Get(CalendarField.DayOfMonth).ToString();
            mes = fecha.GetDisplayName(2, 2, Locale.Default);
            tvFecha.Text = string.Format(dia + ", " + mes);
            lockScreenInstance = this;
        }

        public void UnlockScreenAndShowNotification()
        {
            Window.AddFlags(WindowManagerFlags.DismissKeyguard);
            Window.AddFlags(WindowManagerFlags.ShowWhenLocked);
            Window.AddFlags(WindowManagerFlags.TurnScreenOn);
        }
       

    }
}