using Android.App;
using Android.OS;
using Android.Hardware;
using Android.Runtime;
using Android.Widget;
using Android.Graphics.Drawables;
using Android;
using Android.Views;
using Java.Util;

namespace LiveDisplay
{
    [Activity(Label = "LockScreenActivity", MainLauncher = false)]

    public class LockScreenActivity : Activity
    {
        
        //Administra los Wallpapers de Android.
        WallpaperManager wallpaperManager = null;
        //variable de tipo Drawable que guardará el Wallpaper.
        Drawable papelTapiz;



        //Antes de iniciar la app.
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            RelativeLayout linearLayout;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LockScreen);
            ActionBar.Hide();
            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;

            linearLayout = (RelativeLayout)FindViewById(Resource.Id.contenedorPrincipal);
            linearLayout.Background=papelTapiz;
            ObtenerFecha();

        }

        //Cuando la app se muestra al Usuario por primera vez.
        protected override void OnResume()
        {
            //StartService(new Android.Content.Intent(this, typeof(AwakeService)));
        }
        protected override void OnPause()
        {
            //Desactivar el listener para ahorrar batería.
            base.OnPause();           
        }

        private void ObtenerFecha()
        {
            string dia, mes= null;

            Calendar fecha = Calendar.GetInstance(Locale.Default);
            dia = fecha.Get(CalendarField.DayOfMonth).ToString();
            mes = fecha.GetDisplayName(2, 2, Locale.Default);

            TextView tvFecha = (TextView)FindViewById(Resource.Id.txtFechaLock);
            tvFecha.Text = string.Format(dia + ", " + mes);
        }

    }
}

