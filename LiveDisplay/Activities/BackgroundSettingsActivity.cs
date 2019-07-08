namespace LiveDisplay.Activities
{    
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
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using System;
    using System.Threading;

    using BlurImage = Com.JackAndPhantom.BlurImage;

    [Activity(Label = "@string/wallpapersettings", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class BackgroundSettingsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private ImageView wallpaperPreview;
        private SeekBar blur;
        private SeekBar opacity;
        private Spinner wallpaperbeingsetted;
        private int currentSpinnerOptionSelected;
        private WallpaperManager wallpaperManager;
        private ConfigurationManager configurationManager;
        private CheckBox appliesToMusicWidget;
        private const int DefaultWallpaperConfig = 0;
        private const int AlbumArtConfig = 1;
        private int defaultBlurLevel, defaultOpacityLevel, albumArtBlurLevel, albumArtOpacityLevel;


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
            appliesToMusicWidget = FindViewById<CheckBox>(Resource.Id.appliesToMusicWidget);
            var spinnerAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.listentriescurrentwallpapersetting, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            wallpaperbeingsetted.Adapter = spinnerAdapter;

            wallpaperbeingsetted.ItemSelected += Wallpaperbeingsetted_ItemSelected;
            appliesToMusicWidget.CheckedChange += AppliesToMusicWidget_CheckedChange;

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
                    LoadConfiguration();
                }
            }
            else
            {
                LoadConfiguration();
            }
        }

        private void AppliesToMusicWidget_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked == true)
            {
                configurationManager.SaveAValue(ConfigurationParameters.AlbumArtBlurLevel, defaultBlurLevel);
                configurationManager.SaveAValue(ConfigurationParameters.AlbumArtOpacityLevel, defaultOpacityLevel);
                configurationManager.SaveAValue(ConfigurationParameters.DefaultWallpaperSettingsAppliesToAlbumArt, true);


            }
            else
            {
                configurationManager.SaveAValue(ConfigurationParameters.DefaultWallpaperSettingsAppliesToAlbumArt, false);

            }
        }

        private void Wallpaperbeingsetted_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            switch(e.Id)
            {
                case DefaultWallpaperConfig:

                    currentSpinnerOptionSelected = (int)e.Id;

                    appliesToMusicWidget.Enabled = true; //If the user tries to set the album artwork opacity and blur
                    blur.Enabled = true;
                    opacity.Enabled = true;


                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        if(Checkers.ThisAppHasReadStoragePermission())
                        using (Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap)
                        {
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(defaultBlurLevel).Async(true);
                            Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            RunOnUiThread(() =>
                            {
                                wallpaperPreview.Background = drawable;
                                wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                            }
                            );
                        }
                    });

                    blur.Progress = defaultBlurLevel;
                    opacity.Progress = defaultOpacityLevel;



                    break;
                case AlbumArtConfig:
                    blur.Enabled = false; //we are disabling it forever, because after several tests It does not work to blur the
                    //Albumart cause causes a crash I cant debug, lost so many hours here.
                    if (appliesToMusicWidget.Checked == true)
                    {
                        appliesToMusicWidget.Enabled = false; //If the user tries to set the album artwork opacity and blur
                                                              //then this checkbox is not anymore valid.
                        blur.Enabled = false;                 //As well as the Seekbars for blur and opacity, because
                        opacity.Enabled = false;              //the Default wallpaper config. also applies to the AlbumArt config.
                                                              //So the user can't slide the seekbars.
                    }

                    currentSpinnerOptionSelected = (int)e.Id;

                    if (appliesToMusicWidget.Checked == true)
                    {
                        albumArtBlurLevel = defaultBlurLevel;
                        albumArtOpacityLevel = defaultOpacityLevel;
                    }

                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        using (Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap)
                        {
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(albumArtBlurLevel).Async(true);
                            Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            RunOnUiThread(() =>
                            {
                                wallpaperPreview.Background = drawable;
                                wallpaperPreview.Background.Alpha = albumArtOpacityLevel;
                            }
                            );
                        }
                    }
                    );

                    blur.Progress = albumArtBlurLevel;
                    opacity.Progress = albumArtOpacityLevel;

                    break;
            }

        }

        private void LoadConfiguration()
        {
            Window.DecorView.SetBackgroundColor(Color.Black);

            defaultBlurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, 1);
            defaultOpacityLevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);
            albumArtBlurLevel= configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, 1);
            albumArtOpacityLevel= configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, 255);
            appliesToMusicWidget.Checked= configurationManager.RetrieveAValue(ConfigurationParameters.DefaultWallpaperSettingsAppliesToAlbumArt);
            ThreadPool.QueueUserWorkItem(m =>
            {
                using (Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap)
                {
                    BlurImage blurImage = new BlurImage(Application.Context);
                    blurImage.Load(bitmap).Intensity(defaultBlurLevel).Async(true);
                    Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                    RunOnUiThread(() =>
                    {
                        wallpaperPreview.Background = drawable;
                        wallpaperPreview.Background.Alpha = defaultBlurLevel;
                    }
                    );
                }
            }
            );

            blur.Progress = defaultBlurLevel;
            opacity.Progress = defaultBlurLevel;
            GC.Collect(0); //Helping the gc, We are manipulating bitmaps.
        }

        private void Opacity_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            wallpaperPreview.Background.Alpha = e.SeekBar.Progress;

            if (currentSpinnerOptionSelected == DefaultWallpaperConfig)
            {
                configurationManager.SaveAValue(ConfigurationParameters.OpacityLevel, e.SeekBar.Progress);
                defaultOpacityLevel = e.SeekBar.Progress;
            }
            else if (currentSpinnerOptionSelected == AlbumArtConfig)
            {
              configurationManager.SaveAValue(ConfigurationParameters.AlbumArtOpacityLevel, e.SeekBar.Progress);
              albumArtOpacityLevel = e.SeekBar.Progress;

            }
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

            if (currentSpinnerOptionSelected == DefaultWallpaperConfig)
            {
                configurationManager.SaveAValue(ConfigurationParameters.BlurLevel, e.SeekBar.Progress);
                defaultBlurLevel = e.SeekBar.Progress;
            }
            else if (currentSpinnerOptionSelected == AlbumArtConfig)
            {
               configurationManager.SaveAValue(ConfigurationParameters.AlbumArtBlurLevel, e.SeekBar.Progress);
               albumArtBlurLevel = e.SeekBar.Progress;
            }
            GC.Collect(0);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1 && grantResults[0] == Permission.Granted)
            {
                LoadConfiguration();
            }
            else
            {
                Finish();
            }
        }
    }
}