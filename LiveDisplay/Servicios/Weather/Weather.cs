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
        public async Task<WeatherRoot> GetWeather()
        {
            string url = "aquivaelendpoint";
            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(json)) return null;

                return DeserializeObject<WeatherRoot>(json);
            }
        }
    }
}