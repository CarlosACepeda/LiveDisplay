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

        BackgroundFactory backgroundFactory = new BackgroundFactory();
        List<ClsNotification> listaNotificaciones = new List<ClsNotification>();
        ActivityLifecycleHelper lifecycleHelper = new ActivityLifecycleHelper();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Propiedades de la ventana: Barra de estado y de Navegación traslúcidas.
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            Window.AddFlags(WindowManagerFlags.TranslucentStatus);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            //Iniciar Lista
            
            InicializarVariables();
            ActionBar.Hide();
            ObtenerFecha();

        }
        protected override void OnResume()
        {
            base.OnResume();
            lifecycleHelper.IsActivityResumed();
            //IniciarLista();

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
            tvFecha.Text = string.Format(dia + ", " + mes);
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
            adapter = new NotificationAdapter(listaNotificaciones);
            recycler.SetAdapter(adapter);
            Log.Info("Adaptador iniciado", "SetAdapter exitoso, lista configurada");
        }
        public void BuildNotification(StatusBarNotification sbn)
        {

            ClsNotification notification = new ClsNotification
            {
                Titulo = sbn.Notification.Extras.GetString("android.title").ToString(),
                Texto = sbn.Notification.Extras.GetString("android.text").ToString(),
                Icono = null
            };
            //<Imposible llamar a 'adapter' aquí, wontfix>

        }
        public void IniciarLista()
        {

            ClsNotification notification = new ClsNotification
            {
                Titulo = "Notificación 1",
                Texto = "Este es un texto de prueba",
                Icono = null
            };
            listaNotificaciones.Add(notification);
        }
    }
}

