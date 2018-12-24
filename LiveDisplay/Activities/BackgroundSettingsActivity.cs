using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Activities
{
    [Activity(Label = "bash", Theme = "@style/LiveDisplayThemeDark")]
    public class BackgroundSettingsActivity : Activity
    {
        ImageView wallpaperPreview;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var wallpaperManager = WallpaperManager.GetInstance(Application.Context);
            
            // Create your application here
            SetContentView(Resource.Layout.BackgroundSettings);
            wallpaperPreview = FindViewById<ImageView>(Resource.Id.wallpaperPreview);
            wallpaperPreview.Background = wallpaperManager.Drawable;
        }
    }
}