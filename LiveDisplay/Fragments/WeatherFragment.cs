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

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ISharedPreferences sharedPreferences = Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private);
            configurationManager = new ConfigurationManager(sharedPreferences);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View v= inflater.Inflate(Resource.Layout.Weather, container, false);
            temperature= v.FindViewById<TextView>(Resource.Id.temperature);
            minimumTemperature = v.FindViewById<TextView>(Resource.Id.minimumtemperature);
            maximumTemperature = v.FindViewById<TextView>(Resource.Id.maximumtemperature);
            TelephonyManager tm = (TelephonyManager)Activity.GetSystemService(Context.TelephonyService);
            string countryCode = tm.NetworkCountryIso;

            ThreadPool.QueueUserWorkItem(async m => 
            {
                var weather= await Weather.GetWeather("Bogotá", countryCode, UnitsFlags.Metric);
                Activity.RunOnUiThread(() =>
                {
                    temperature.Text = weather?.MainWeather.Temperature.ToString();
                    minimumTemperature.Text = weather?.MainWeather.MinTemperature.ToString();
                    maximumTemperature.Text = weather?.MainWeather.MaxTemperature.ToString();
                });
            });
            return v;
        }
    }
}