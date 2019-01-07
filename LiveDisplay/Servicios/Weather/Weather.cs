using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Newtonsoft.Json.JsonConvert;

using LiveDisplay.DataRepository;

namespace LiveDisplay.Servicios.Weather
{
    class Weather
    {
        //This class will be the one that connects to the api and provide Lockscreen with Weather information.
        public static async Task<WeatherRoot> GetWeather(string city, string country)
        {
            string url = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0},{1}&appid=9ca11a6f4426446b991ff390d4f7430f", city, country);
            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(json)) return null;

                WeatherRoot weatherRoot=
                DeserializeObject<WeatherRoot>(json);
                return weatherRoot;
            }
        }
    }
}