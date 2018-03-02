using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.Adapters;
using LiveDisplay.Databases;
using LiveDisplay.Factories;
using LiveDisplay.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LockScreenActivity : Activity
    {
        public static LockScreenActivity lockScreenInstance;
        private WallpaperManager wallpaperManager = null;
        private Drawable papelTapiz;
        private RelativeLayout relativeLayout;

        private RecyclerView recycler;
        private RecyclerView.LayoutManager layoutManager;
        private NotificationAdapter adapter;
        private BackgroundFactory backgroundFactory = new BackgroundFactory();
        private List<ClsNotification> listaNotificaciones = new List<ClsNotification>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            lockScreenInstance = this;

            //Propiedades de la ventana: Barra de estado odulta y de Navegación traslúcida
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            //Bring data from Database.
            InicializarVariables();
            InitData();
            ThreadPool.QueueUserWorkItem(o => ObtenerFecha());
        }

        protected override void OnResume()
        {
            InitData();
            base.OnResume();
        }

        protected override void OnPause()
        {
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
            relativeLayout = (RelativeLayout)FindViewById(Resource.Id.contenedorPrincipal);
            wallpaperManager = WallpaperManager.GetInstance(this);

            papelTapiz = wallpaperManager.Drawable;
            relativeLayout.Background = backgroundFactory.Difuminar(papelTapiz);
            recycler = (RecyclerView)FindViewById(Resource.Id.NotificationListRecyclerView);
            layoutManager = new LinearLayoutManager(this);
            recycler.SetLayoutManager(layoutManager);
        }

        private void InitData()
        {
            DBHelper db = new DBHelper();
            listaNotificaciones = db.SelectTableNotification();
            adapter = new NotificationAdapter(listaNotificaciones);
            RunOnUiThread(() => recycler.SetAdapter(adapter));
        }

        public void InsertItem(ClsNotification notification)
        {
            listaNotificaciones.Add(notification);
            if (adapter != null && recycler != null)
            {
                RunOnUiThread(() => adapter.NotifyDataSetChanged());
            }
        }

        public void RemoveItem(ClsNotification notification)
        {
            int indice = listaNotificaciones.IndexOf(listaNotificaciones.First(o => o.Id == notification.Id));

            listaNotificaciones.RemoveAt(indice);

            if (adapter != null && recycler != null)
            {
                RunOnUiThread(() => adapter.NotifyItemRemoved(indice));
            }
        }
    }
}