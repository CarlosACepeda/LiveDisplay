using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios;
using Com.JackAndPhantom;
using LiveDisplay.Misc;
using Android.Graphics;
using Android.Graphics.Drawables;
using System.Threading;

namespace LiveDisplay.Activities
{
    [Activity(Label = "bash", Theme = "@style/LiveDisplayThemeDark")]
    public class BackgroundSettingsActivity : Activity
    {
        ImageView wallpaperPreview;
        SeekBar blur;
        SeekBar opacity;
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
            LoadPreviousValues();
        }

        private void LoadPreviousValues()
        {
            int savedblurlevel= configurationManager.RetrieveAValue(ConfigurationParameters.blurlevel, 1);
            int savedOpacitylevel= configurationManager.RetrieveAValue(ConfigurationParameters.opacitylevel, 255);

            Bitmap bitmap = ((BitmapDrawable)wallpaperManager.Drawable).Bitmap;
            

            BlurImage blurImage = new BlurImage(Application.Context);
                blurImage.Load(bitmap).Intensity(savedblurlevel).Async(true);
                Drawable drawable = new BitmapDrawable(Resources, blurImage.GetImageBlur());
                wallpaperPreview.Background = drawable;

            wallpaperPreview.Background.Alpha = savedOpacitylevel;

            blur.SetProgress(savedblurlevel, false);
            opacity.SetProgress(savedOpacitylevel, false);
            GC.Collect(0); //Helping the gc, We are manipulating bitmaps.

        }

        private void Opacity_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            wallpaperPreview.Background.Alpha = e.SeekBar.Progress;
            configurationManager.SaveAValue(ConfigurationParameters.opacitylevel, e.SeekBar.Progress);
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
            configurationManager.SaveAValue(ConfigurationParameters.blurlevel, e.SeekBar.Progress);
            GC.Collect(0);
        }

    }
}