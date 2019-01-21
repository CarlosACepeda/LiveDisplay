using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.JackAndPhantom;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;

namespace LiveDisplay.Activities
{
    [Activity(Label = "bash", Theme = "@style/LiveDisplayThemeDark")]
    public class BackgroundSettingsActivity : Activity
    {
        private ImageView wallpaperPreview;
        private SeekBar blur;
        private SeekBar opacity;
        private WallpaperManager wallpaperManager;
        private ConfigurationManager configurationManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            wallpaperManager = WallpaperManager.GetInstance(Application.Context);

            configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context));

            // Create your application here
            SetContentView(Resource.Layout.BackgroundSettings);
            wallpaperPreview = FindViewById<ImageView>(Resource.Id.wallpaperPreview);
            blur = FindViewById<SeekBar>(Resource.Id.blur);
            opacity = FindViewById<SeekBar>(Resource.Id.opacity);
            opacity.Max = 255;
            blur.Max = 25;
            blur.StopTrackingTouch += Blur_StopTrackingTouch;
            opacity.StopTrackingTouch += Opacity_StopTrackingTouch;
            if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
            {
                if (Application.Context.CheckSelfPermission("android.permission.READ_EXTERNAL_STORAGE") != Permission.Granted)
                {
                    RequestPermissions(new string[1] { "android.permission.READ_EXTERNAL_STORAGE" }, 1);
                }
                else
                {
                    LoadPreviousValues();
                }
            }
            else
            {
                LoadPreviousValues();
            }
        }

        private void LoadPreviousValues()
        {
            Window.DecorView.SetBackgroundColor(Color.Black);

            int savedblurlevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, 1);
            int savedOpacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);

            Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;

            BlurImage blurImage = new BlurImage(Application.Context);
            blurImage.Load(bitmap).Intensity(savedblurlevel).Async(true);
            Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
            wallpaperPreview.Background = drawable;

            wallpaperPreview.Background.Alpha = savedOpacitylevel;

            blur.Progress = savedblurlevel;
            opacity.Progress = savedOpacitylevel;
            GC.Collect(0); //Helping the gc, We are manipulating bitmaps.
        }

        private void Opacity_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            wallpaperPreview.Background.Alpha = e.SeekBar.Progress;
            configurationManager.SaveAValue(ConfigurationParameters.OpacityLevel, e.SeekBar.Progress);
        }

        private void Blur_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            Drawable drawable = null;
            wallpaperManager.ForgetLoadedWallpaper();
            Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;
            BlurImage blurImage = new BlurImage(Application.Context);
            blurImage.Load(bitmap).Intensity(e.SeekBar.Progress).Async(true);
            drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
            wallpaperPreview.Background = drawable;
            configurationManager.SaveAValue(ConfigurationParameters.BlurLevel, e.SeekBar.Progress);
            GC.Collect(0);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1 && grantResults[0] == Permission.Granted)
            {
                LoadPreviousValues();
            }
            else
            {
                Finish();
            }
        }
    }
}