using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Weather;

namespace LiveDisplay.Fragments
{
    public class WeatherFragment : Fragment
    {
        ConfigurationManager configurationManager;

        TextView temperature;
        TextView minimumTemperature;
        TextView maximumTemperature;
        TextView city;
        TextView humidity;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ISharedPreferences sharedPreferences = Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private);
            configurationManager = new ConfigurationManager(sharedPreferences);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v= inflater.Inflate(Resource.Layout.Weather, container, false);
            temperature= v.FindViewById<TextView>(Resource.Id.temperature);
            minimumTemperature = v.FindViewById<TextView>(Resource.Id.minimumtemperature);
            maximumTemperature = v.FindViewById<TextView>(Resource.Id.maximumtemperature);
            city = v.FindViewById<TextView>(Resource.Id.city);
            humidity = v.FindViewById<TextView>(Resource.Id.humidity);
            TelephonyManager tm = (TelephonyManager)Activity.GetSystemService(Context.TelephonyService);
            string countryCode = tm.NetworkCountryIso;

            string thecity = configurationManager.RetrieveAValue(ConfigurationParameters.City, "");
            UnitsFlags unitsFlags = UnitsFlags.Metric;
            string temperatureSuffix = "°C";
            if (configurationManager.RetrieveAValue(ConfigurationParameters.UseImperialSystem) == true)
            {
                unitsFlags = UnitsFlags.Imperial;
                temperatureSuffix = "°F";
            }

            ThreadPool.QueueUserWorkItem(async m => 
            {
                var weather= await Weather.GetWeather(thecity, countryCode, unitsFlags);
                Activity.RunOnUiThread(() =>
                {
                    temperature.Text = weather?.MainWeather.Temperature.ToString()+ temperatureSuffix;
                    minimumTemperature.Text = weather?.MainWeather.MinTemperature.ToString() + temperatureSuffix;
                    maximumTemperature.Text = weather?.MainWeather.MaxTemperature.ToString() + temperatureSuffix; 
                    city.Text = weather?.Name + ": " + weather?.Weather[0].Description;
                    humidity.Text = weather?.MainWeather.Humidity.ToString();
                    
                });
            });
            return v;
        }
    }
}