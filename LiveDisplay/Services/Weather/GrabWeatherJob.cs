using Android.Content;
using Android.Util;
using AndroidX.Work;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Awake;
using System;

namespace LiveDisplay.Servicios.Weather
{
    public class GrabWeatherJob : Worker
    {
        public static event EventHandler<bool> WeatherUpdated;

        public GrabWeatherJob(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {
        }

        public override Result DoWork()
        {
            if (AwakeHelper.GetAwakeStatus() == AwakeStatus.Sleeping || AwakeHelper.GetAwakeStatus() == AwakeStatus.SleepingWithDeviceMotionEnabled)
            {
                return Result.InvokeSuccess(); //We want to keep the job running but don't do the job itself while Awake is sleeping.
            }

            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Weather);

            string city = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, "New York");
            string country = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCountryCode, "us");
            string unit = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherTemperatureMeasureUnit, "metric");

            var result = OpenWeatherMapClient.GetWeather(city, country, unit);
            if (result != null)
            {
                Log.Info("LiveDisplay", "Job Result Sucess");
                WeatherUpdated?.Invoke(null, true);
                return Result.InvokeSuccess();
            }
            else
            {
                Log.Info("LiveDisplay", "Job Result Not Sucess");
                WeatherUpdated?.Invoke(null, false);
                return Result.InvokeRetry();
            }
        }
    }
}