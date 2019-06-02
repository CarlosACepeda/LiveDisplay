using Android.App;
using Android.Content;
using Android.Support.V7.Preferences;
using LiveDisplay.DataRepository;
using LiveDisplay.Misc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

namespace LiveDisplay.Servicios.Weather
{
    internal class Weather
    {
        private readonly static ConfigurationManager configurationManager = new ConfigurationManager(Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private));

        //This class will be the one that connects to the api and provide Lockscreen with Weather information.
        public static async Task<WeatherRoot> GetWeather(string city, string country, UnitsFlags unitsformat)
        {
            string units = "";
            switch (unitsformat)
            {
                case UnitsFlags.Metric:
                    units = "metric";
                    break;

                case UnitsFlags.Imperial:
                    units = "imperial";
                    break;

                default:
                    units = "metric";
                    break;
            }

            string url = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0},{1}&units={2}&appid=9ca11a6f4426446b991ff390d4f7430f", city, country, units);
            using (var client = new HttpClient())
            {
                try
                {
                    var json = await client.GetStringAsync(url);

                    if (string.IsNullOrWhiteSpace(json)) return null;


                    WeatherRoot weatherRoot =
                    DeserializeObject<WeatherRoot>(json);                    

                    configurationManager.SaveAValue(ConfigurationParameters.WeatherCity, weatherRoot.Name);
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherDescription, weatherRoot.Weather[0].Description);
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherHumidity, weatherRoot.MainWeather.Humidity.ToString()+ "%");
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherLastUpdated, DateTime.Now.ToString("ddd"+ "," + "hh:mm"));
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherMaximum, weatherRoot.MainWeather.MaxTemperature.ToString());
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherMaximum, weatherRoot.MainWeather.MinTemperature.ToString());

                    string unitsuffix = "°k";
                    switch (unitsformat)
                    {
                        case UnitsFlags.Imperial:
                            unitsuffix = "°f";
                            break;
                        case UnitsFlags.Metric:
                            unitsuffix = "°c";
                            break;
                    }
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherTemperatureUnit, unitsuffix);
                    return weatherRoot;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}