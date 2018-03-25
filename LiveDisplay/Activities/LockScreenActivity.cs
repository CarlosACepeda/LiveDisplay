using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Transitions;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.Adapters;
using LiveDisplay.Factories;
using LiveDisplay.Servicios;
using System;

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
        private NotificationAdapter adapter;
        private BackgroundFactory backgroundFactory;
        private TextView tvTitulo;
        private TextView tvTexto;
        private PendingIntent notificationAction;
        string dia, mes = null;
        private Calendar fecha;
        private TextView tvFecha;

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
            string titulo = Catcher.listaNotificaciones[position].Notification.Extras.GetString("android.title");
            if (titulo != null)
            {
                tvTitulo.Text = titulo;
            }
            string text = Catcher.listaNotificaciones[position].Notification.Extras.GetString("android.text");
            if (text != null)
            {
                tvTexto.Text = text;
            }
            v.Visibility = ViewStates.Visible;
            notificationAction = Catcher.listaNotificaciones[position].Notification.ContentIntent;
        }
        public void OnItemLongClick(int position)
        {
            if (Catcher.listaNotificaciones[position].IsClearable == true)
            {
                int notiId = Catcher.listaNotificaciones[position].Id;
                string notiTag = Catcher.listaNotificaciones[position].Tag;
                string notiPack = Catcher.listaNotificaciones[position].PackageName;
                NotificationSlave slave = new NotificationSlave();
                slave.CancelNotification(notiPack, notiTag, notiId);
                v.Visibility = ViewStates.Invisible;
            }
            else
            {
                Toast.MakeText(Application.Context, "No", ToastLength.Short).Show();
            }
        }
        private void CheckDataChanges()
        {
            if (oldListSize != newListSize)
            {
                adapter.NotifyDataSetChanged();
            }
        }
        private void OnNotificationClicked(object sender, System.EventArgs e)
        {
            notificationAction.Send();
            v.Visibility = ViewStates.Invisible;
        }
        private void BindViews()
        {
            linearLayout = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            recycler = FindViewById<RecyclerView>(Resource.Id.NotificationListRecyclerView);
            tvTitulo = (TextView)FindViewById(Resource.Id.tvTitulo);
            tvTexto = (TextView)FindViewById(Resource.Id.tvTexto);
            v = FindViewById<View>(Resource.Id.fragment1);
            tvFecha = (TextView)FindViewById(Resource.Id.txtFechaLock);
            
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
            adapter = null;
            lockScreenInstance = null;
            backgroundFactory = null;
            fecha = null;
            dia = null;
            mes = null;
            notificationAction = null;
        }
        private void BindClickEvents()
        {
            v.Click += OnNotificationClicked;
        }
        private void UnbindClickEvents()
        {
            v.Click -= OnNotificationClicked;
        }
        private void InicializarValores()
        {
            //Propiedades de la ventana: Barra de estado odulta y de Navegación traslúcida
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;
            backgroundFactory = new BackgroundFactory();
            linearLayout.Background = backgroundFactory.Difuminar(papelTapiz);
            layoutManager = new LinearLayoutManager(this);
            recycler.SetLayoutManager(layoutManager);
            //Innceseario(?)
            adapter = Catcher.adapter;
            RunOnUiThread(() => recycler.SetAdapter(adapter));
            fecha = Calendar.GetInstance(Locale.Default);
            dia = fecha.Get(CalendarField.DayOfMonth).ToString();
            mes = fecha.GetDisplayName(2, 2, Locale.Default);
            tvFecha.Text = string.Format(dia + ", " + mes);
            lockScreenInstance = this;
        }
    }
}