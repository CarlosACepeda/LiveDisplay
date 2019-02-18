using LiveDisplay.DataRepository;
using LiveDisplay.Misc;
using System.Net.Http;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

namespace LiveDisplay.Servicios.Weather
{
    internal class Weather
    {
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