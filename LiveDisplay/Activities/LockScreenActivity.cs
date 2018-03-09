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
        int oldListSize, newListSize = 0;
        public static LockScreenActivity lockScreenInstance;
        private WallpaperManager wallpaperManager = null;
        private Drawable papelTapiz;
        private LinearLayout linearLayout;
        private TextView tvTitulo;
        private TextView tvTexto;
        private View v;
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
            InicializarVariables();
            ThreadPool.QueueUserWorkItem(o => ObtenerFecha());
        }

        protected override void OnResume()
        {
            newListSize = listaNotificaciones.Count;
            CheckDataChanges();
            base.OnResume();
        }

        protected override void OnPause()
        {
            oldListSize = listaNotificaciones.Count;
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
            tvTitulo = FindViewById<TextView>(Resource.Id.tvTitulo);
            tvTexto = FindViewById<TextView>(Resource.Id.tvTexto);
            v = FindViewById<View>(Resource.Id.fragment1);

            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;
            linearLayout.Background = backgroundFactory.Difuminar(papelTapiz);
            layoutManager = new LinearLayoutManager(this);
            recycler.SetLayoutManager(layoutManager);
            DBHelper db = new DBHelper();
            listaNotificaciones = db.SelectTableNotification();
            adapter = new NotificationAdapter(listaNotificaciones);
            adapter.ItemClick += OnItemClick;
            RunOnUiThread(() => recycler.SetAdapter(adapter));
        }

        private void CheckDataChanges()
        {

            if (oldListSize != newListSize)
            {
                adapter.NotifyDataSetChanged();
            }
        }

        public void InsertItem(ClsNotification notification)
        {
            listaNotificaciones.Add(notification);
            tvTitulo.Text = notification.Titulo;
            tvTexto.Text = notification.Texto;
            if (adapter != null && recycler != null)
            {
                RunOnUiThread(() => adapter.NotifyItemInserted(listaNotificaciones.Count - 1));
            }
        }

        public void RemoveItem(ClsNotification notification)
        {
            int indice = listaNotificaciones.IndexOf(listaNotificaciones.First(o => o.Id == notification.Id));

            listaNotificaciones.RemoveAt(indice);

            if (adapter != null && recycler != null)
            {
                RunOnUiThread(() => adapter.NotifyItemRemoved(indice));
                v.Visibility = ViewStates.Invisible;
            }
        }

        void OnItemClick(object sender, int position)
        {
            //Arreglame: A veces el Fragment pierde los valores.
            int notifNum = position + 1;
            //Mostrar el Fragment          
            if (v.Visibility == ViewStates.Invisible)
            {
                v.Visibility = ViewStates.Visible;
            }
            else
            {
                v.Visibility = ViewStates.Invisible;
            }
            
        }
    }
}