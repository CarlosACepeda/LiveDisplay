using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Activities;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;

using Preference = AndroidX.Preference.Preference;

namespace LiveDisplay.Fragments
{
    public class PreferencesFragment : AndroidX.Preference.PreferenceFragmentCompat
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
        }

        private void WallpaperSettingsPreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(BackgroundSettingsActivity))))
            {
                StartActivity(intent);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.prefs);
            AndroidX.Preference.PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.prefs, false);
        }
    }
}