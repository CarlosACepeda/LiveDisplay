using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Android.Views;
using AndroidX.Preference;
using AndroidX.Work;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Weather;
using System;

namespace LiveDisplay.Fragments.Preferences
{
    public class WeatherSettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Weather);
        private ISharedPreferences sharedPreferences = Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            PreferenceManager.SharedPreferencesName = "weatherpreferences";
            PreferenceManager.SharedPreferencesMode = (int)FileCreationMode.Private;
            AddPreferencesFromResource(Resource.Xml.weather_widget_prefs);
            PreferenceManager.SetDefaultValues(Application.Context, "weatherpreferences", (int)FileCreationMode.Private, Resource.Xml.weather_widget_prefs, true);
            string city = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, string.Empty);
            if (city != string.Empty)
            {
                Preference weatherCity = FindPreference("weathercity");
                weatherCity.Summary = city;
            }
            string country = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCountryCode, string.Empty);
            if (country == string.Empty)
            {
                TelephonyManager tm = (TelephonyManager)Activity.GetSystemService(Context.TelephonyService);
                string countryCode = tm.NetworkCountryIso;
                configurationManager.SaveAValue(ConfigurationParameters.WeatherCountryCode, countryCode);
            }
            country = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCountryCode, string.Empty);
            if (country != string.Empty)
            {
                Preference weatherCountry = FindPreference("weathercountrycode");
                weatherCountry.Summary = country;
            }
            string interval = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherUpdateFrequency, "-1");
            if (interval != "-1")
            {
                ListPreference weatherUpdateFrequency = FindPreference("weatherupdatefrequency") as ListPreference;
                weatherUpdateFrequency.Value = interval;
                weatherUpdateFrequency.Summary = weatherUpdateFrequency.Entry;
            }

            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnResume()
        {
            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
            base.OnResume();
        }

        public override void OnPause()
        {
            sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
            base.OnPause();
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            switch (key)
            {
                case ConfigurationParameters.WeatherUpdateFrequency:
                    int interval_minutes = int.Parse(sharedPreferences.GetString(ConfigurationParameters.WeatherUpdateFrequency, "-1"));
                    if (interval_minutes != -1)
                    {
                        ListPreference weatherUpdateFrequency = FindPreference("weatherupdatefrequency") as ListPreference;
                        weatherUpdateFrequency.Value = interval_minutes.ToString();
                        weatherUpdateFrequency.Summary = weatherUpdateFrequency.Entry;

                        PeriodicWorkRequest weatherPeriodicWorkRequest = PeriodicWorkRequest.Builder.From<GrabWeatherJob>(TimeSpan.FromMinutes(interval_minutes)).Build();
                        WorkManager.GetInstance(this.Activity).Enqueue(weatherPeriodicWorkRequest);
                    }
                    else
                    {
                        WorkManager.GetInstance(this.Activity).CancelAllWork();
                    }

                    break;

                case ConfigurationParameters.WeatherCity:

                    Preference weatherCity = FindPreference("weathercity");
                    weatherCity.Summary = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, string.Empty);
                    break;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}