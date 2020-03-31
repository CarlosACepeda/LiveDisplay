using Android.Content;
using Android.Util;
using AndroidX.Work;
using LiveDisplay.Misc;

namespace LiveDisplay.Servicios.Weather
{
    public class GrabWeatherJob : Worker
    {
        public GrabWeatherJob(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {

        }
        public override Result DoWork()
        {
            if (AwakeService.GetAwakeStatus() == Awake.AwakeStatus.Sleeping)
            {
                return Result.InvokeSuccess(); //We want to keep the job running but don't do the job itself while Awake is sleeping.
            }

            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Weather);

            string city = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, "New York");
            string country = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCountryCode, "us");
            string unit = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherTemperatureMeasureUnit, "metric");

            var result = Weather.GetWeather(city, country, unit);
            if (result != null)
            {
                Log.Info("LiveDisplay", "Job Result Sucess");
                return Result.InvokeSuccess();
            }
            else
            {
                Log.Info("LiveDisplay", "Job Result Not Sucess");
                return Result.InvokeRetry();
            }
            

        }
    }
}