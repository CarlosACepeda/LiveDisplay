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
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
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
        private BackgroundFactory backgroundFactory = new BackgroundFactory();
        private TextView tvTitulo;
        private TextView tvTexto;
        private PendingIntent notificationAction;

        protected override void OnCreate(Bundle savedInstanceState)
        {           

            base.OnCreate(savedInstanceState);
            lockScreenInstance = this;

            //Propiedades de la ventana: Barra de estado odulta y de Navegación traslúcida
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            InicializarVariables();
            ThreadPool.QueueUserWorkItem(o => ObtenerFecha());
        }

        protected override void OnResume()
        {
            try
            {
                newListSize = Catcher.listaNotificaciones.Count;
            }
            catch
            {
                Toast.MakeText(ApplicationContext, "¡No puedo mostrar notificaciones, el servicio debe estar Activado usando el menu de ajustes de esta app", ToastLength.Long).Show();
            }

            CheckDataChanges();
            base.OnResume();
        }

        protected override void OnPause()
        {
            try
            {
                oldListSize = Catcher.listaNotificaciones.Count;
            }
            catch

            {
            };

            base.OnPause();
        }

        private void ObtenerFecha()
        {
            string dia, mes = null;

            Calendar fecha = Calendar.GetInstance(Locale.Default);
            dia = fecha.Get(CalendarField.DayOfMonth).ToString();
            mes = fecha.GetDisplayName(2, 2, Locale.Default);
            TextView tvFecha = (TextView)FindViewById(Resource.Id.txtFechaLock);
            RunOnUiThread(() => tvFecha.Text = string.Format(dia + ", " + mes));
        }

        private void InicializarVariables()
        {
            linearLayout = FindViewById<LinearLayout>(Resource.Id.contenedorPrincipal);
            recycler = FindViewById<RecyclerView>(Resource.Id.NotificationListRecyclerView);
            tvTitulo = (TextView)FindViewById(Resource.Id.tvTitulo);
            tvTexto = (TextView)FindViewById(Resource.Id.tvTexto);

            v = FindViewById<View>(Resource.Id.fragment1);
            v.Click += OnNotificationClicked;

            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;
            linearLayout.Background = backgroundFactory.Difuminar(papelTapiz);
            FindViewById<ViewGroup>(Resource.Id.contenedorPrincipal).LayoutTransition.EnableTransitionType(Android.Animation.LayoutTransitionType.Changing);
            layoutManager = new LinearLayoutManager(this);
            recycler.SetLayoutManager(layoutManager);
            adapter = Catcher.adapter;
            RunOnUiThread(() => recycler.SetAdapter(adapter));
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
    }
}