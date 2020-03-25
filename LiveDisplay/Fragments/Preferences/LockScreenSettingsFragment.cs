using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using LiveDisplay.Activities;

namespace LiveDisplay.Fragments.Preferences
{    
    public class LockScreenSettingsFragment : AndroidX.Preference.PreferenceFragmentCompat
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.lockscreen_prefs);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.lockscreen_prefs, false);

            Preference wallpapersettingspreference = FindPreference("wallpapersettings");
            Preference weathersettingspreference = FindPreference("weathersettings");

            wallpapersettingspreference.PreferenceClick += WallpaperSettingsPreference_PreferenceClick;
            weathersettingspreference.PreferenceClick += Weathersettingspreference_PreferenceClick;


        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
        private void WallpaperSettingsPreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(BackgroundSettingsActivity))))
            {
                StartActivity(intent);
            }
        }
        private void Weathersettingspreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(WeatherSettingsActivity))))
            {
                StartActivity(intent);
            }
        }

    }
}