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
    public class PreferencesFragment : AndroidX.Preference.PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private ISharedPreferences sharedPreferences = AndroidX.Preference.PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        private bool isSleepstarttimesetted = false;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
        }

        private void PreferencesFragmentCompat_timepicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            //Simple trick to save two different values using the same timepicker.

            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
            if (isSleepstarttimesetted)
            {
                configurationManager.SaveAValue(ConfigurationParameters.FinishSleepTime, string.Concat(e.HourOfDay.ToString() + e.Minute.ToString()));
            }
            else
            {
                configurationManager.SaveAValue(ConfigurationParameters.StartSleepTime, string.Concat(e.HourOfDay.ToString() + e.Minute.ToString()));
                isSleepstarttimesetted = true;
            }
        }

        private void Inactivehourssettingspreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            using (TimePickerDialog datePickerDialog = new TimePickerDialog(Activity, PreferencesFragmentCompat_timepicked, DateTime.Now.Hour, DateTime.Now.Minute, false))
            {
                if (!isSleepstarttimesetted)
                {
                    Toast.MakeText(Activity, "Set the Start hour", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(Activity, "Set the finish hour", ToastLength.Long).Show();
                }
                //datePickerDialog.Create();
                datePickerDialog.Show();
            }
        }

        private void Weathersettingspreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            using (Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(WeatherSettingsActivity))))
            {
                StartActivity(intent);
            }
        }

        private void Githubprojectpreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            string url = "https://github.com/CarlosACepeda/LiveDisplay/";
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse(url));
            StartActivity(intent);
        }

        private void Blacklistpreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(BlacklistActivity)));
            StartActivity(intent);
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
            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();
            sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            switch (key)
            {
                case ConfigurationParameters.DoubleTapOnTopActionBehavior:
                    Preference doubletaptopbehavior = FindPreference("doubletapontoppactionbehavior");
                    switch (sharedPreferences.GetString(ConfigurationParameters.DoubleTapOnTopActionBehavior, "0"))
                    {
                        case "0":
                            doubletaptopbehavior.SetSummary(Resource.String.doubletaptopactiondesc);
                            break;

                        case "1":
                            doubletaptopbehavior.SetSummary(Resource.String.doubletaptopactioninverteddesc);
                            break;
                    }
                    break;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.prefs);
            AndroidX.Preference.PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.prefs, false);
            Preference wallpapersettingspreference = FindPreference("wallpapersettings");
            Preference githubprojectpreference = FindPreference("contributetoproject");
            Preference blacklistpreference = FindPreference("blacklist");
            Preference weathersettingspreference = FindPreference("weathersettings");
            Preference inactivehourssettingspreference = FindPreference("inactivetimesettings");
            wallpapersettingspreference.PreferenceClick += WallpaperSettingsPreference_PreferenceClick;
            blacklistpreference.PreferenceClick += Blacklistpreference_PreferenceClick;
            githubprojectpreference.PreferenceClick += Githubprojectpreference_PreferenceClick;
            weathersettingspreference.PreferenceClick += Weathersettingspreference_PreferenceClick;
            inactivehourssettingspreference.PreferenceClick += Inactivehourssettingspreference_PreferenceClick;
        }
    }
}