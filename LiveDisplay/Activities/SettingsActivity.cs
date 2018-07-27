using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios;
using LiveDisplay.Misc;
using System;
using Android.Runtime;
using Android.Graphics.Drawables;
using Java.IO;
using Android.Graphics;
using Android.Provider;
using Android.Util;
using LiveDisplay.Factories;
using LiveDisplay.Fragments;
using static Android.Preferences.Preference;
using Android.Preferences;

namespace LiveDisplay.Activities
{
    //prepare for deprecation, this will be Settings Screen
    [Activity(Label = "@string/settings", Theme ="@style/LiveDisplayTheme.NoActionBar")]
    public class SettingsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private int requestCode = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            

            // Create your application here
            SetContentView(Resource.Layout.Settings);
            using (toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
            }
                
            
            FragmentManager.BeginTransaction().Replace(Resource.Id.content, new LiveDisplayPreferencesFragment()).Commit();
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (this.requestCode == requestCode && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;
                try
                {
                    
                    BackgroundFactory background = new BackgroundFactory();
                    background.SaveImagePath(uri);
                    background = null;

                }
                catch
                {

                }
            }
        }
        
        //TODO: replace this event handler to handle PReferenceScreen click, instead.
        private void BtnChangeWallpaper_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Pick image"), requestCode);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        
    }
}