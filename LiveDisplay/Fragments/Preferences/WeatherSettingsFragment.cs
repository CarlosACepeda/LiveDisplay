using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Preference;
using AndroidX.Work;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using LiveDisplay.Services.Weather;
using System;

namespace LiveDisplay.Fragments.Preferences
{
    public class WeatherSettingsFragment : PreferenceFragmentCompat
    {
        ConfigurationManager configurationManager = new ConfigurationManager();

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.weather_prefs);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.weather_prefs, true);
            SharedPreferenceListenerService.ConfigurationChanged += SharedPreferenceListenerService_ConfigurationChanged; ;
        }

        private void SharedPreferenceListenerService_ConfigurationChanged(object sender, Services.Configuration.ConfigurationChangedEventArgs e)
        {
            if(e.Key== ConfigurationParameters.WeatherUpdateInterval)
            {

                if ((int)e.Value== Application.Context.Resources.GetInteger(Resource.Integer.zero_minutes))
                {
                    if(WorkManager.IsInitialized)
                        WorkManager.GetInstance(Application.Context).CancelAllWorkByTag(GrabWeatherJob.Tag);
                }
                else
                {
                    if (!Checkers.ThisAppCanReadLocation())
                    {
                        configurationManager.SaveAValue(ConfigurationParameters.WeatherUpdateInterval,
                            Application.Context.Resources.GetInteger(Resource.Integer.zero_minutes));
                        AskForLocationPermission();
                    }
                    else
                    {
                        PeriodicWorkRequest weatherPeriodicWorkRequest = PeriodicWorkRequest.Builder
                            .From<GrabWeatherJob>(TimeSpan.FromMinutes((int)e.Value))
                            .AddTag(GrabWeatherJob.Tag).Build();

                        WorkManager.GetInstance(Application.Context).Enqueue(weatherPeriodicWorkRequest);
                    }
                }
            }
        }

        private void AskForLocationPermission()
        {
            var intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.Location);
            intent.PutExtras(extras);

            Activity.StartActivityForResult(intent, Permissions.Location);
            
        }
        public override void OnDestroy()
        {
            SharedPreferenceListenerService.ConfigurationChanged -= SharedPreferenceListenerService_ConfigurationChanged; ;
            base.OnDestroy();
        }
    }
}