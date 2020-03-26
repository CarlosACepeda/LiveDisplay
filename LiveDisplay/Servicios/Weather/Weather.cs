using Android.App;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Javax.Security.Auth;
using LiveDisplay.DataRepository;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

namespace LiveDisplay.Servicios.Weather
{
    internal class Weather
    {
        private readonly static ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Weather);
        private static string imageURL = "http://openweathermap.org/img/wn/{0}@2x.png";

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

                    configurationManager.SaveAValue(ConfigurationParameters.WeatherCurrent, weatherRoot.MainWeather.Temperature.ToString());
                    string unitsuffix = "°k";
                    switch (measurementunit)
                    {
                        case MeasurementUnits.Fahrenheit:
                            unitsuffix = "°F";
                            break;

                        case MeasurementUnits.Celsius:
                            unitsuffix = "°C";
                            break;
                        case MeasurementUnits.Kelvin:
                            unitsuffix = "°K";
                            break;
                    }
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherTemperatureUnit, unitsuffix);

                    using (var imageGrabClient = new HttpClient())
                    {
                        try
                        {
                            var stream = await imageGrabClient.GetStreamAsync(string.Format(imageURL, weatherRoot.Weather[0].Icon));
                            Drawable drawable = Drawable.CreateFromStream(stream, "image");

                            if (configurationManager.RetrieveAValue(ConfigurationParameters.WeatherUpdateChangesWallpaper))
                            {
                                WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                                {
                                    BlurLevel = 5,
                                    OpacityLevel = 100,
                                    SecondsOfAttention = 5,
                                    WallpaperPoster = WallpaperPoster.Weather,
                                    Wallpaper = (BitmapDrawable)drawable
                                });
                            }
                        }
                        catch
                        {                            
                            Toast.MakeText(Application.Context, "FAILED DOWNLOAD IMAGE", ToastLength.Long).Show();
                        }

                    }


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