using Android.App;
using Android.OS;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Views;
using Java.Util;
using Android.Service.Notification;
using LiveDisplay.Factories;
using System.Collections.Generic;
using LiveDisplay.Objects;
using Android.Support.V7.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Misc;
using Java.Lang;
using System.Threading.Tasks;
using Android.Util;
using Android.Content;
using System;
using LiveDisplay.Databases;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "LockScreen", MainLauncher = false)]
    public class LockScreenActivity : Activity
    {
        WallpaperManager wallpaperManager = null;
        Drawable papelTapiz;
        RelativeLayout relativeLayout;

        RecyclerView recycler;
        RecyclerView.LayoutManager layoutManager;
        NotificationAdapter adapter;
        DBHelper db = new DBHelper();
        BackgroundFactory backgroundFactory = new BackgroundFactory();
        public List<ClsNotification> listaNotificaciones = new List<ClsNotification>();
        ActivityLifecycleHelper lifecycleHelper = new ActivityLifecycleHelper();       

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Propiedades de la ventana: Barra de estadocodulta y de Navegación traslúcida
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
            lifecycleHelper.IsActivityPaused();            
        }
        private void ObtenerFecha()
        {
            string dia, mes = null;

            Calendar fecha = Calendar.GetInstance(Locale.Default);
            dia = fecha.Get(CalendarField.DayOfMonth).ToString();
            mes = fecha.GetDisplayName(2, 2, Locale.Default);
            TextView tvFecha = (TextView)FindViewById(Resource.Id.txtFechaLock);
            RunOnUiThread(()=>tvFecha.Text = string.Format(dia + ", " + mes));
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
            
            listaNotificaciones = db.SelectTableNotification();
            adapter = new NotificationAdapter(listaNotificaciones);
            RunOnUiThread(() => recycler.SetAdapter(adapter));
            lifecycleHelper.IsActivityResumed();
        }

    }
}