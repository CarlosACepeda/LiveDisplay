using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;

namespace LiveDisplay.Servicios.Weather
{
    class GrabWeatherJob : JobService
    {

        public override bool OnStartJob(JobParameters @params)
        {
            ConfigurationManager configurationManager = new ConfigurationManager(Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private));

            string city = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, "New York");
            string country = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCountryCode, "us");
            string unit = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherTemperatureMeasureUnit, "metric");
            Task.Run(() =>
            {
                var result = Weather.GetWeather(city, country, unit);
                JobFinished(@params, false);
            });
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            return false;
        }
    }
}