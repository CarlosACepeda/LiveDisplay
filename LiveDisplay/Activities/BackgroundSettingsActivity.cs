namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Runtime;
    using Android.Util;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using LiveDisplay.Factories;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Wallpaper;
    using System;
    using System.Threading;

    [Activity(Label = "@string/wallpapersettings", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class BackgroundSettingsActivity : AppCompatActivity
    {
        private Button pickwallpaper, enableblurandroid10;
        private TextView enableblurandroid10warning;
        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private AppCompatImageView wallpaperPreview;
        private SeekBar blur;
        private SeekBar opacity;
        private Spinner wallpaperbeingsetted;
        private int currentSpinnerOptionSelected;
        private WallpaperManager wallpaperManager;
        private ConfigurationManager configurationManager;
        private CheckBox appliesToMusicWidget;
        private const int WallpaperConfig = 0;
        private const int AlbumArtConfig = 1;
        private const int CustomWallpaperConfig = 2;
        private int defaultBlurLevel, defaultOpacityLevel, albumArtBlurLevel, albumArtOpacityLevel;

        private readonly int REQUEST_CODE_PICKWALLPAPER = 2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            wallpaperManager = WallpaperManager.GetInstance(Application.Context);

            configurationManager = new ConfigurationManager(AppPreferences.Default);

            // Create your application here
            SetContentView(Resource.Layout.BackgroundSettings);

            using (toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
            }

            pickwallpaper = FindViewById<Button>(Resource.Id.pickwallpaper);
            wallpaperPreview = FindViewById<AppCompatImageView>(Resource.Id.wallpaperPreview);
            blur = FindViewById<SeekBar>(Resource.Id.blur);
            opacity = FindViewById<SeekBar>(Resource.Id.opacity);
            wallpaperbeingsetted = FindViewById<Spinner>(Resource.Id.wallpaperbeingsetted);
            appliesToMusicWidget = FindViewById<CheckBox>(Resource.Id.appliesToMusicWidget);
            if(Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                enableblurandroid10 = FindViewById<Button>(Resource.Id.enableblurandroid10);
                enableblurandroid10warning = FindViewById<TextView>(Resource.Id.warningblurandroid10);
                if (configurationManager.RetrieveAValue(ConfigurationParameters.BlurEnabledForAndroid10))
                {
                   enableblurandroid10.Text = Resources.GetString(Resource.String.disable);
                }
                else
                {
                   enableblurandroid10.Text = Resources.GetString(Resource.String.enable);
                }
                enableblurandroid10warning.Visibility = Android.Views.ViewStates.Visible;
                enableblurandroid10.Visibility = Android.Views.ViewStates.Visible;
                enableblurandroid10.Click += Enableblurandroid10_Click;
            }
            var spinnerAdapter = ArrayAdapter<string>.CreateFromResource(this, Resource.Array.listentriescurrentwallpapersetting, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            wallpaperbeingsetted.Adapter = spinnerAdapter;

            wallpaperbeingsetted.ItemSelected += Wallpaperbeingsetted_ItemSelected;
            pickwallpaper.Click += Pickwallpaper_Click;
            appliesToMusicWidget.CheckedChange += AppliesToMusicWidget_CheckedChange;

            opacity.Max = 255;
            blur.Max = 25;
            blur.StopTrackingTouch += Blur_StopTrackingTouch;
            opacity.StopTrackingTouch += Opacity_StopTrackingTouch;

            //Precondition: Background must be black so we can set the opacity correctly of the wallpaper when being set.
            Window.DecorView.SetBackgroundColor(Color.Black);

            if (!Checkers.ThisAppHasReadStoragePermission())
            {
                Toast.MakeText(this, "You need the Storage permission", ToastLength.Long).Show();
                StartActivity(new Intent(this, typeof(MainActivity)));
            }
            LoadConfiguration();
        }

        private void Enableblurandroid10_Click(object sender, EventArgs e)
        {
            if(!configurationManager.RetrieveAValue(ConfigurationParameters.BlurEnabledForAndroid10))
            {
                configurationManager.SaveAValue(ConfigurationParameters.BlurEnabledForAndroid10, true);
                if (enableblurandroid10 != null)
                    enableblurandroid10.Text = Resources.GetString(Resource.String.disable);
            }
            else
            {
                configurationManager.SaveAValue(ConfigurationParameters.BlurEnabledForAndroid10, false);
                if (enableblurandroid10 != null)
                    enableblurandroid10.Text = Resources.GetString(Resource.String.enable);
            }
        }

        private void Pickwallpaper_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            using (AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(button.Context))
            {
                int currentwallpapersetted = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"));
                builder.SetTitle(Resources.GetString(Resource.String.changewallpaper));
                builder.SetSingleChoiceItems(new string[] { button.Context.GetString(Resource.String.defaultwallpaper), button.Context.GetString(Resource.String.pickwallpaper) }, currentwallpapersetted, OnDialogClickEventArgs);
                builder.Create();
                builder.Show();
            }
        }

        private void OnDialogClickEventArgs(object sender, DialogClickEventArgs e)
        {
            switch (e.Which)
            {
                case 0: //0 is 'Default wallpaper.'
                    blur.Enabled = true;
                    opacity.Enabled = true;
                    appliesToMusicWidget.Enabled = true;

                    ThreadPool.QueueUserWorkItem(m =>
                        {
                            defaultBlurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, defaultBlurLevel);
                            defaultOpacityLevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, defaultOpacityLevel);

                            Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;

                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(defaultBlurLevel);
                            Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                            RunOnUiThread(() =>
                            {
                                wallpaperPreview.Background = drawable;
                                wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                            }
                            );
                        });
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "1");

                    break;

                case 1: //1 is 'Pick a wallpaper'
                    blur.Enabled = true;
                    opacity.Enabled = true;
                    appliesToMusicWidget.Enabled = true;

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
            if (e.IsChecked)
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
            //This call is necessary because when this event handler is attached, for some reason is being called
            //Like if the user had clicked a item. why? idk.
            //if this method is called before the user has the ReadStorage permission the app will crash.
            //in that case simply do nothing.
            if (Checkers.ThisAppHasReadStoragePermission())
            {
                currentSpinnerOptionSelected = (int)e.Id;

                switch (currentSpinnerOptionSelected)
                {
                    case WallpaperConfig:

                        if (!pickwallpaper.Enabled) pickwallpaper.Enabled = true;

                        switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
                        {
                            case "0":
                                ThreadPool.QueueUserWorkItem(m =>
                                {
                                    wallpaperManager.ForgetLoadedWallpaper();
                                    Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;

                                    BlurImage blurImage = new BlurImage(Application.Context);
                                    blurImage.Load(bitmap).Intensity(defaultBlurLevel);
                                    Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                                    RunOnUiThread(() =>
                                    {
                                        wallpaperPreview.Background = drawable;
                                        wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                                    }
                                    );
                                });
                                blur.Enabled = true;
                                blur.SetProgress(defaultBlurLevel, true);
                                opacity.Enabled = true;
                                opacity.SetProgress(defaultOpacityLevel, true);
                                appliesToMusicWidget.Enabled = true; //If the user tries to set the album artwork opacity and blur
                                break;

                            case "1":
                                ThreadPool.QueueUserWorkItem(m =>
                                {
                                    var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                                    using (var backgroundcopy = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath)))
                                    {
                                        BlurImage blurImage = new BlurImage(Application.Context);
                                        blurImage.Load(backgroundcopy).Intensity(defaultBlurLevel);
                                        var drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                                        RunOnUiThread(() =>
                                        {
                                            wallpaperPreview.Background = drawable;
                                            wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                                        });
                                    }
                                });

                                blur.Enabled = true;
                                blur.SetProgress(defaultBlurLevel, true);
                                opacity.Enabled = true;
                                opacity.SetProgress(defaultOpacityLevel, true);
                                appliesToMusicWidget.Enabled = true; //If the user tries to set the album artwork opacity and blur

                                break;
                        }

                        blur.Progress = defaultBlurLevel;
                        opacity.Progress = defaultOpacityLevel;

                        break;

                    case AlbumArtConfig:
                        pickwallpaper.Enabled = false;
                        appliesToMusicWidget.Enabled = false;
                        blur.Enabled = true;
                        opacity.Enabled = true;

                        if (appliesToMusicWidget.Checked)
                        {
                            //If the user tries to set the album artwork opacity and blur
                            //then this checkbox is not anymore valid.
                            blur.Enabled = false;                 //As well as the Seekbars for blur and opacity, because
                            opacity.Enabled = false;              //the Default wallpaper config. also applies to the AlbumArt config.
                                                                  //So the user can't slide the seekbars.
                        }

                        currentSpinnerOptionSelected = (int)e.Id;

                        if (appliesToMusicWidget.Checked)
                        {
                            albumArtBlurLevel = defaultBlurLevel;
                            albumArtOpacityLevel = defaultOpacityLevel;
                        }

                        ThreadPool.QueueUserWorkItem(m =>
                        {
                            Bitmap bitmap = null;
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                            {
                                bitmap = ((BitmapDrawable)Application.Context.GetDrawable(Resource.Drawable.album_artwork)).Bitmap;
                            }
                            else
                            {
                                bitmap = ((BitmapDrawable)Application.Context.Resources.GetDrawable(Resource.Drawable.album_artwork)).Bitmap;
                            }
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(bitmap).Intensity(albumArtBlurLevel);
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
        }

        private void LoadConfiguration()
        {
            defaultBlurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.BlurLevel, ConfigurationParameters.DefaultBlurLevel);
            defaultOpacityLevel = configurationManager.RetrieveAValue(ConfigurationParameters.OpacityLevel, ConfigurationParameters.DefaultOpacityLevel);
            albumArtBlurLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtBlurLevel, ConfigurationParameters.DefaultAlbumartBlurLevel);
            albumArtOpacityLevel = configurationManager.RetrieveAValue(ConfigurationParameters.AlbumArtOpacityLevel, ConfigurationParameters.DefaultAlbumartOpacityLevel);
            appliesToMusicWidget.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.DefaultWallpaperSettingsAppliesToAlbumArt);
            blur.Progress = defaultBlurLevel;
            opacity.Progress = defaultBlurLevel;

            switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
            {
                case "0":
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        wallpaperManager.ForgetLoadedWallpaper();
                        Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;

                        BlurImage blurImage = new BlurImage(Application.Context);
                        blurImage.Load(bitmap).Intensity(defaultBlurLevel);
                        Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                        RunOnUiThread(() =>
                        {
                            wallpaperPreview.Background = drawable;
                            wallpaperPreview.Background.Alpha = defaultOpacityLevel;
                        }
                        );
                    });
                    break;

                case "1":
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                        using (var backgroundcopy = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath)))
                        {
                            BlurImage blurImage = new BlurImage(Application.Context);
                            blurImage.Load(backgroundcopy).Intensity(defaultBlurLevel);
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
        }

        private void Opacity_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            if (wallpaperPreview.Background != null)
                wallpaperPreview.Background.Alpha = e.SeekBar.Progress;

            if (currentSpinnerOptionSelected == WallpaperConfig || currentSpinnerOptionSelected == CustomWallpaperConfig)
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
            if (e.SeekBar.Progress == 0) return;

            Drawable drawable = null;

            if (currentSpinnerOptionSelected == WallpaperConfig)
            {
                Bitmap wallpaper = null;
                switch (configurationManager.RetrieveAValue(ConfigurationParameters.ChangeWallpaper, "0"))
                {
                    case "0": //Default Wallpaper (system)
                        wallpaperManager.ForgetLoadedWallpaper();
                        wallpaper = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;
                        break;

                    case "1": //User picked a custom wallpaper.
                        var imagePath = configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, "");
                        wallpaper = BitmapFactory.DecodeFile(configurationManager.RetrieveAValue(ConfigurationParameters.ImagePath, imagePath));
                        break;
                }
                ThreadPool.QueueUserWorkItem(m =>
                {
                    using (wallpaper)
                    {
                        BlurImage blurImage = new BlurImage(Application.Context);
                        blurImage.Load(wallpaper).Intensity(e.SeekBar.Progress);
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
                defaultBlurLevel = e.SeekBar.Progress;
            }
            else if (currentSpinnerOptionSelected == AlbumArtConfig)
            {
                ThreadPool.QueueUserWorkItem(m =>
                {
                    using (var albumart = BitmapFactory.DecodeResource(Resources, Resource.Drawable.album_artwork))
                    {
                        BlurImage blurImage = new BlurImage(Application.Context);
                        blurImage.Load(albumart).Intensity(e.SeekBar.Progress);
                        drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                        RunOnUiThread(() =>
                        {
                            var previousAlpha = wallpaperPreview.Background.Alpha;
                            wallpaperPreview.Background = drawable;
                            wallpaperPreview.Background.Alpha = previousAlpha;
                        });
                    }
                });

                configurationManager.SaveAValue(ConfigurationParameters.AlbumArtBlurLevel, e.SeekBar.Progress);
                albumArtBlurLevel = e.SeekBar.Progress;
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
                            blurImage.Load(bitmap).Intensity(defaultBlurLevel);
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
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "1"); //System wallpaper.
                }
            }
            else
            {
                Log.Info("LiveDisplay", "Data was null");
                configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "1"); //System wallpaper.
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}