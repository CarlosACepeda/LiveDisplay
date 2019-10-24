namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Preferences;
    using Android.Runtime;
    using Android.Support.V7.App;
    using Android.Util;
    using Android.Views;
    using Android.Widget;
    using LiveDisplay.Factories;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using System;
    using System.Threading;

    using BlurImage = Com.JackAndPhantom.BlurImage;

    [Activity(Label = "@string/wallpapersettings", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class BackgroundSettingsActivity : AppCompatActivity
    {
        private Button pickwallpaper;
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
        private const int CustomWallpaperConfig = 2;
        private int defaultBlurLevel, defaultOpacityLevel, albumArtBlurLevel, albumArtOpacityLevel;

        private int REQUEST_CODE_READ_STORAGE_PERMISSION = 1;
        private int REQUEST_CODE_PICKWALLPAPER = 2;

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

            pickwallpaper = FindViewById<Button>(Resource.Id.pickwallpaper);
            wallpaperPreview = FindViewById<ImageView>(Resource.Id.wallpaperPreview);
            blur = FindViewById<SeekBar>(Resource.Id.blur);
            opacity = FindViewById<SeekBar>(Resource.Id.opacity);
            wallpaperbeingsetted = FindViewById<Spinner>(Resource.Id.wallpaperbeingsetted);
            appliesToMusicWidget = FindViewById<CheckBox>(Resource.Id.appliesToMusicWidget);
            var spinnerAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.listentriescurrentwallpapersetting, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            wallpaperbeingsetted.Adapter = spinnerAdapter;

            wallpaperbeingsetted.ItemSelected += Wallpaperbeingsetted_ItemSelected;
            pickwallpaper.Click += Pickwallpaper_Click;
            appliesToMusicWidget.CheckedChange += AppliesToMusicWidget_CheckedChange;

            opacity.Max = 255;
            blur.Max = 25;
            blur.StopTrackingTouch += Blur_StopTrackingTouch;
            opacity.StopTrackingTouch += Opacity_StopTrackingTouch;
             if (Checkers.ThisAppHasReadStoragePermission()==false)
             {
                 RequestPermissions(new string[1] { "android.permission.READ_EXTERNAL_STORAGE" }, REQUEST_CODE_READ_STORAGE_PERMISSION);
             }
             else
             {
                 LoadConfiguration();
             }
        }

        private void Pickwallpaper_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            using (Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(button.Context))
            {
                int currentwallpapersetted = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"));
                builder.SetTitle(Resources.GetString(Resource.String.changewallpaper));
                builder.SetSingleChoiceItems(new string[] { button.Context.GetString(Resource.String.blackwallpaper), button.Context.GetString(Resource.String.defaultwallpaper), button.Context.GetString(Resource.String.pickwallpaper) }, currentwallpapersetted,  OnDialogClickEventArgs);
                builder.Create();
                builder.Show();
            }
        }

        private void OnDialogClickEventArgs(object sender, DialogClickEventArgs e)
        {
            switch (e.Which)
            {
                case 0: //0 is 'Black wallpaper'
                    //Picking black wallpaper will avoid the user to control the Blur and opacity. (why does the user need to anyway);) 
                    wallpaperPreview.SetBackgroundColor(Color.Black);
                    blur.Enabled = false;
                    opacity.Enabled = false;
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "0");

                    break;
                case 1: //1 is 'Default wallpaper.'
                    blur.Enabled = true;
                    opacity.Enabled = true;
                    ThreadPool.QueueUserWorkItem(m =>
                        {
                            defaultBlurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, 1);
                            defaultOpacityLevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);

                            Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;

                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(defaultBlurLevel).Async(true);
                            Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            RunOnUiThread(() =>
                            {
                                wallpaperPreview.Background = drawable;
                                wallpaperPreview.Background.Alpha = defaultBlurLevel;
                            }
                            );

                        });
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "1");



                    break;
                case 2: //2 is 'Pick one wallpaper'
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "2");
                    
                    using (Intent intent = new Intent())
                    {
                        intent.SetType("image/*");
                        intent.SetAction(Intent.ActionGetContent);
                        StartActivityForResult(Intent.CreateChooser(intent, "Pick image"), REQUEST_CODE_PICKWALLPAPER);
                        //In the result we'll save the correct value if the user has picked a image also we'll use the image that
                        //is coming from the result as the background of this activity

                    }

                    break;
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

                    switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
                    {
                        case "0":
                            wallpaperPreview.SetBackgroundColor(Color.Black);
                            blur.Enabled = false;
                            opacity.Enabled = false;
                            break;

                        case "1":
                            ThreadPool.QueueUserWorkItem(m =>
                            {
                                wallpaperManager.ForgetLoadedWallpaper();
                                Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;

                                BlurImage blurImage = new BlurImage(Application.Context);
                                blurImage.Load(bitmap).Intensity(defaultBlurLevel).Async(true);
                                Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                                RunOnUiThread(() =>
                                {
                                    wallpaperPreview.Background = drawable;
                                    wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                                }
                                );

                            });
                            break;
                        case "2":
                            ThreadPool.QueueUserWorkItem(m =>
                            {
                                var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                                using (var backgroundcopy = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath)))
                                {
                                    BlurImage blurImage = new BlurImage(Application.Context);
                                    blurImage.Load(backgroundcopy).Intensity(defaultBlurLevel).Async(true);
                                    var drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                                    RunOnUiThread(() =>
                                    {
                                        wallpaperPreview.Background = drawable;
                                        wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                                    });
                                }
                            });

                            break;
                    }

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
                        Bitmap bitmap = ((BitmapDrawable)Application.Context.GetDrawable(Resource.Drawable.album_artwork)).Bitmap;
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
                    );

                    blur.Progress = albumArtBlurLevel;
                    opacity.Progress = albumArtOpacityLevel;

                    break;
            }

        }

        private void LoadConfiguration()
        {
            defaultBlurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, 1);
            defaultOpacityLevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);
            albumArtBlurLevel= configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, 1);
            albumArtOpacityLevel= configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, 255);
            appliesToMusicWidget.Checked= configurationManager.RetrieveAValue(ConfigurationParameters.DefaultWallpaperSettingsAppliesToAlbumArt);
            blur.Progress = defaultBlurLevel;
            opacity.Progress = defaultBlurLevel;

            switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
            {
                case "0":
                    wallpaperPreview.SetBackgroundColor(Color.Black);
                    blur.Enabled = false;
                    opacity.Enabled = false;
                    break;
                case "1":
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        wallpaperManager.ForgetLoadedWallpaper();
                        Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;

                        BlurImage blurImage = new BlurImage(Application.Context);
                        blurImage.Load(bitmap).Intensity(defaultBlurLevel).Async(true);
                        Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                        RunOnUiThread(() =>
                        {
                            wallpaperPreview.Background = drawable;
                            wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                        }
                        );

                    });
                    break;
                case "2":
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                        using (var backgroundcopy = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath)))
                        {
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(backgroundcopy).Intensity(defaultBlurLevel).Async(true);
                            var drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            RunOnUiThread(() =>
                            {
                                wallpaperPreview.Background = drawable;
                                wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                            });
                        }
                    });

                    break;
            }

            
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
            else if (currentSpinnerOptionSelected == CustomWallpaperConfig)
            {
                configurationManager.SaveAValue(ConfigurationParameters.OpacityLevel, e.SeekBar.Progress);
                defaultOpacityLevel = e.SeekBar.Progress;
            }

        }

        private void Blur_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            Drawable drawable = null;

            switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
            {
                case "1": //Default Wallpaper
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
                    break;
                case "2":
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                        using (var backgroundcopy = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath)))
                        {
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(backgroundcopy).Intensity(e.SeekBar.Progress).Async(true);
                            drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            RunOnUiThread(() =>
                            {
                                var previousAlpha = wallpaperPreview.Background.Alpha;
                                wallpaperPreview.Background = drawable;
                                wallpaperPreview.Background.Alpha = previousAlpha;
                            });
                        }
                    });
                    break;
            }

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
            else if (currentSpinnerOptionSelected == CustomWallpaperConfig)
            {
                configurationManager.SaveAValue(ConfigurationParameters.BlurLevel, e.SeekBar.Progress);
                defaultBlurLevel = e.SeekBar.Progress;

            }
            GC.Collect(0);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == REQUEST_CODE_READ_STORAGE_PERMISSION && grantResults[0] == Permission.Denied)
            {
                Toast.MakeText(this,"You must allow LiveDisplay to read storage to retrieve images", ToastLength.Long).Show();
                Finish();
            }
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;
                try
                {
                    BackgroundFactory background = new BackgroundFactory();
                    background.SaveImagePath(uri);
                    background = null;
                    Log.Info("tag", "Path sent to BackgroundFactory");
                    var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                    if (imagePath != "")
                    {
                        ThreadPool.QueueUserWorkItem(m =>
                        {
                            Bitmap bitmap = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath));

                            defaultBlurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, 1);
                            defaultOpacityLevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, 255);
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(defaultBlurLevel).Async(true);
                            Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            RunOnUiThread(() =>
                            {
                                wallpaperPreview.Background = drawable;
                                wallpaperPreview.Background.Alpha = defaultBlurLevel;
                            }
                            );


                        });
                    }
                }
                catch
                {
                    
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "0"); //Black wallpaper.
                }
            }
            else
            {
                Log.Info("LiveDisplay", "Data was null");
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "0"); //Black wallpaper.
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

    }
}