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
        private readonly static ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Weather);

        //This class will be the one that connects to the api and provide Lockscreen with Weather information.
        public static async Task<WeatherRoot> GetWeather(string city, string country, string measurementunit)
        {
            string url = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0},{1}&units={2}&appid=9ca11a6f4426446b991ff390d4f7430f", city, country, measurementunit);
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
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherHumidity, weatherRoot.MainWeather.Humidity.ToString() + "%");
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherLastUpdated, DateTime.Now.ToString("ddd" + "," + "hh:mm"));
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherMaximum, weatherRoot.MainWeather.MaxTemperature.ToString());
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherMaximum, weatherRoot.MainWeather.MinTemperature.ToString());
                    string temperatureSuffix = "--";
                    switch (measurementunit)
                    {
                        case "imperial":
                            temperatureSuffix = "°f";
                            break;

                        case "metric":
                            temperatureSuffix = "°c";
                            break;
                    }

                    configurationManager.SaveAValue(ConfigurationParameters.WeatherCurrent, weatherRoot.MainWeather.Temperature.ToString() + temperatureSuffix);
                    string unitsuffix = "°k";
                    switch (measurementunit)
                    {
                        case "imperial":
                            unitsuffix = "°f";
                            break;

                        case "metric":
                            unitsuffix = "°c";
                            break;
                    }
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherTemperatureUnit, unitsuffix); //??????
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