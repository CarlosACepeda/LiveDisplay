using Android.App;
using Android.OS;
using Android.Hardware;
using Android.Runtime;
using Android.Widget;
using Android.Graphics.Drawables;
using Android;
using Android.Views;

namespace LiveDisplay
{
    [Activity(Label = "LockScreenActivity", MainLauncher = true)]

    public class LockScreenActivity : Activity , ISensorEventListener
    {
        //administra los sensores de Android.
        SensorManager sensorManager = null;
        //Variable la cuál guardará la constante del Sensor de Proximidad.
        Sensor sProximidad;
        //Administra los Wallpapers de Android.
        WallpaperManager wallpaperManager = null;
        //variable de tipo Drawable que guardará el Wallpaper.
        Drawable papelTapiz;


        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            //Nada todavía.
        }

        //..Cuando detecta un cambio en el sensor.
        public void OnSensorChanged(SensorEvent evento)
        {
            if (evento.Values[0] > 1)
            {
                Toast.MakeText(this, "El sensor detecta proximidad", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, "Algo sucede...", ToastLength.Long).Show();
            }
        }

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
            linearLayout.SetBackgroundDrawable(papelTapiz);
            //Asignación de variables, sensorManager al Servicio SensorService, sProximidad al sensor SensorType.Proximity
            sensorManager = (SensorManager)GetSystemService(SensorService);
            sProximidad = sensorManager.GetDefaultSensor(SensorType.Proximity);

        }
        
        //Cuando la app se muestra al Usuario por primera vez.
        protected override void OnResume()
        {
            //Registrar un Listener para Oir los eventos del Sensor sProximidad.
            base.OnResume();
            sensorManager.RegisterListener(this, sProximidad, SensorDelay.Fastest);
        }
        protected override void OnPause()
        {
            //Desactivar el listener para ahorrar batería.
            base.OnPause();
            sensorManager.UnregisterListener(this);
        }
    }
}

