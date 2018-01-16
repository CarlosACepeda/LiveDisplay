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

namespace LiveDisplay
{
    [Activity(Label = "LockScreenActivity", MainLauncher = false)]

   
    public class LockScreenActivity : ListActivity
    {
        WallpaperManager wallpaperManager = null;
        Drawable papelTapiz;
        RelativeLayout relativeLayout;

        //Los siguientes campos son para la notificación.
        TextView tvTituloNotificacion, tvTextoNotificacion, tvIconoNotificacion = new TextView(Application.Context);

        BackgroundFactory backgroundFactory = new BackgroundFactory();
        List<ClsNotification> listaNotificaciones = new List<ClsNotification>();
        ClsNotification notification = new ClsNotification();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Propiedades de la ventana: Barra de estado y de Navegación traslúcidas.
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            Window.AddFlags(WindowManagerFlags.TranslucentStatus);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            InicializarVariables();
            ActionBar.Hide();
            ObtenerFecha();
            
            
        }
        protected override void OnResume()
        {
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
            tvFecha.Text = string.Format(dia + ", " + mes);
        }
        private void InicializarVariables()
        {
            relativeLayout = (RelativeLayout)FindViewById(Resource.Id.contenedorPrincipal);
            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;
            relativeLayout.Background = backgroundFactory.Difuminar(papelTapiz);
            
        }
        public void BuildNotification(StatusBarNotification sbn)
        {
            notification.Titulo = sbn.Notification.Extras.GetString("android.title");
            notification.Texto = sbn.Notification.Extras.GetString("android.text");
            notification.Icono = sbn.Notification.Extras.GetInt("android.icon");
            //Handle Event
            listaNotificaciones.Add(notification);
        }
        
    }
}

