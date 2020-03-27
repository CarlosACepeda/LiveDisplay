using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Util;
using AndroidX.Work;
using LiveDisplay.Misc;
using System.Threading.Tasks;

namespace LiveDisplay.Servicios.Weather
{
    public class GrabWeatherJob : Worker
    {
        bool success = false;
        public GrabWeatherJob(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {

        }
        public override Result DoWork()
        {
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