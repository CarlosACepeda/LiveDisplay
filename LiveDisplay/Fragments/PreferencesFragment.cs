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

namespace LiveDisplay.Fragments
{
    public class PreferencesFragmentCompat : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        private bool isSleepstarttimesetted = false;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.prefs);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.prefs, false);
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

        private void PreferencesFragmentCompat_timepicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            //Simple trick to save two different values using the same timepicker.

            ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences);
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
            using (TimePickerDialog datePickerDialog = new TimePickerDialog(this.Context, PreferencesFragmentCompat_timepicked, DateTime.Now.Hour, DateTime.Now.Minute, false))
            {
                if (!isSleepstarttimesetted)
                {
                    Toast.MakeText(this.Context, "Set the Start hour", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(this.Context, "Set the finish hour", ToastLength.Long).Show();
                }
                datePickerDialog.Create();
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

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case 2:
                    if (resultCode == Result.Ok && data != null)
                    {
                        Android.Net.Uri uri = data.Data;
                        try
                        {
                            BackgroundFactory background = new BackgroundFactory();
                            background.SaveImagePath(uri);
                            background = null;
                            Log.Info("tag", "Path sent to BackgroundFactory");
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        Log.Info("LiveDisplay", "Data was null");
                        using (ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences))
                        {
                            configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "0");
                        }
                    }

                    break;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1 && grantResults[0] == Permission.Granted)
            {
                //Nothing.
            }
            else
            {
                Log.Info("LiveDisplay", "User did not allow the read storage permision, reverting back to Black wallpaper");
                using (ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences))
                {
                    configurationManager.SaveAValue(ConfigurationParameters.ChangeWallpaper, "0");
                }
            }
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            switch (key)
            {
                case ConfigurationParameters.ChangeWallpaper:
                    switch (sharedPreferences.GetString(ConfigurationParameters.ChangeWallpaper, "0"))
                    {
                        case "1":

                            if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
                            {
                                if (Application.Context.CheckSelfPermission("android.permission.READ_EXTERNAL_STORAGE") != Permission.Granted)
                                {
                                    RequestPermissions(new string[1] { "android.permission.READ_EXTERNAL_STORAGE" }, 1);
                                }
                            }

                            break;

                        case "2":

                            using (Intent intent = new Intent())
                            {
                                intent.SetType("image/*");
                                intent.SetAction(Intent.ActionGetContent);
                                //here 1 means the request code.
                                StartActivityForResult(Intent.CreateChooser(intent, "Pick image"), 2);
                            }
                            break;
                    }
                    break;
            }
        }
    }
}