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
    [Activity(Label = "@string/settings", Theme ="@style/LiveDisplayTheme.NoActionBar")]
    public class SettingsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;

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
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        
    }
}