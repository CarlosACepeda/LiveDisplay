using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.JackAndPhantom;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;
using System.Threading;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/wallpapersettings", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class BackgroundSettingsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private ImageView wallpaperPreview;
        private SeekBar blur;
        private SeekBar opacity;
        private Spinner wallpaperbeingsetted;
        private WallpaperManager wallpaperManager;
        private ConfigurationManager configurationManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            wallpaperManager = WallpaperManager.GetInstance(Application.Context);

            configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context));

            // Create your application here
            SetContentView(Resource.Layout.BackgroundSettings);

            using (toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
            }

            wallpaperPreview = FindViewById<ImageView>(Resource.Id.wallpaperPreview);
            blur = FindViewById<SeekBar>(Resource.Id.blur);
            opacity = FindViewById<SeekBar>(Resource.Id.opacity);
            wallpaperbeingsetted = FindViewById<Spinner>(Resource.Id.wallpaperbeingsetted);
            var spinnerAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.listentriescurrentwallpapersetting, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            wallpaperbeingsetted.Adapter = spinnerAdapter;

            wallpaperbeingsetted.ItemSelected += Wallpaperbeingsetted_ItemSelected;

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

        private void Wallpaperbeingsetted_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
        }

        private void LoadPreviousValues()
        {
            Window.DecorView.SetBackgroundColor(Color.Black);

            int savedblurlevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, 1);
            int savedOpacitylevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);

            ThreadPool.QueueUserWorkItem(m =>
            {
                using (Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap)
                {
                    BlurImage blurImage = new BlurImage(Application.Context);
                    blurImage.Load(bitmap).Intensity(savedblurlevel).Async(true);
                    Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                    RunOnUiThread(() =>
                    {
                        wallpaperPreview.Background = drawable;
                        wallpaperPreview.Background.Alpha = savedOpacitylevel;
                    }
                    );
                }
            }
            );

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
            ThreadPool.QueueUserWorkItem(m =>
            {
                using (var backgroundcopy = (BitmapDrawable)wallpaperManager.Drawable)
                {
                    BlurImage blurImage = new BlurImage(Application.Context);
                    blurImage.Load(backgroundcopy.Bitmap).Intensity(e.SeekBar.Progress).Async(true);
                    drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                    RunOnUiThread(() =>
                    {
                        var previousAlpha = wallpaperPreview.Background.Alpha;
                        wallpaperPreview.Background = drawable;
                        wallpaperPreview.Background.Alpha = previousAlpha;
                    });
                }
            });
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