using Android.App;
using Android.App.Job;
using Android.Content;
using LiveDisplay.Misc;
using System;
using System.Threading.Tasks;

namespace LiveDisplay.Servicios.Weather
{
    [Service(Name = "com.underground.livedisplay.DownloadJob",
         Permission = "android.permission.BIND_JOB_SERVICE")]
    public class GrabWeatherJob : JobService
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