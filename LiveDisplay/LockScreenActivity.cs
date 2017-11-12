using Android.App;
using Android.OS;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Views;
using Java.Util;
using Android.Graphics;
using Android.Renderscripts;

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
            linearLayout = (RelativeLayout)FindViewById(Resource.Id.contenedorPrincipal);
            wallpaperManager = WallpaperManager.GetInstance(this);
            papelTapiz = wallpaperManager.Drawable;
            linearLayout.Background = Difuminar(papelTapiz);
            ObtenerFecha();

        }

        //Cuando la app se muestra al Usuario por primera vez.
        protected override void OnResume()
        {

        }
        protected override void OnPause()
        {
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
        public Drawable Difuminar(Drawable papelTapiz)
        {

            Bitmap originalBitmap = ((BitmapDrawable)papelTapiz).Bitmap;
            // Create another bitmap that will hold the results of the filter.
            Bitmap blurredBitmap;
            blurredBitmap = Bitmap.CreateBitmap(originalBitmap);


            //Crear la instancia de RenderScript que hará el trabajo
            RenderScript rs = RenderScript.Create(this);
            //Alocar memoria para que RenderScript trabaje.
            Allocation input = Allocation.CreateFromBitmap(rs, originalBitmap, Allocation.MipmapControl.MipmapFull, AllocationUsage.Script);
            Allocation output = Allocation.CreateTyped(rs, input.Type);

            // Load up an instance of the specific script that we want to use.
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));
            script.SetInput(input);

            // Set the blur radius
            script.SetRadius(25);

            // Start the ScriptIntrinisicBlur
            script.ForEach(output);

            // Copy the output to the blurred bitmap
            output.CopyTo(blurredBitmap);

            Drawable papelTapizDifuminado = new BitmapDrawable(blurredBitmap);
            return papelTapizDifuminado;

        }

    }
}

