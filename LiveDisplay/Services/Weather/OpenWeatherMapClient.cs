using Android.App;
using Android.Graphics.Drawables;
using Android.Widget;
using LiveDisplay.DataRepository;
using LiveDisplay.Misc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

namespace LiveDisplay.Services.Weather
{
    internal class OpenWeatherMapClient
    {
        private readonly static ConfigurationManager configurationManager = new ConfigurationManager();
        private const string ImageURL = "http://openweathermap.org/img/wn/{0}@2x.png";
        const string StreamSourceName = "image";
        const string ApiKey = "9ca11a6f4426446b991ff390d4f7430f"; //Why are you leaving API keys in the plain? TODO: Delete API Key and use a Vault to administer Api values, fortunately this is a Free API key.
        const string OpenWeatherMapForecastUrl = "http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&units={2}&appid={3}&lang={4}";
        public static async Task<WeatherRoot> GetWeather(string lat, string lon, string measurementunit, string language)
        {
            string url = string.Format(OpenWeatherMapForecastUrl, lat, lon, measurementunit, ApiKey, language);
            using (var client = new HttpClient())
            {
                try
                {
                    var json = await client.GetStringAsync(url);

                    if (string.IsNullOrWhiteSpace(json)) return null;

                    string measurementUnitRepresentation = string.Empty;
                    switch(measurementunit)
                    {
                        case MeasurementUnits.Kelvin:
                            measurementUnitRepresentation = MeasurementUnits.KelvinRepresentation;
                            break;
                        case MeasurementUnits.Celsius:
                            measurementUnitRepresentation = MeasurementUnits.CelsiusRepresentation;
                            break;
                        case MeasurementUnits.Fahrenheit:
                            measurementUnitRepresentation = MeasurementUnits.FahrenheitRepresentation;
                            break;

                    }

                    WeatherRoot weatherRoot =
                    DeserializeObject<WeatherRoot>(json);
                    configurationManager.SaveAValue(ConfigurationParameters.CurrentTemperature, weatherRoot.MainWeather.Temperature+ measurementUnitRepresentation);
                    configurationManager.SaveAValue(ConfigurationParameters.CityForCurrentWeatherForecast, weatherRoot.Name);
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherDescription, weatherRoot.Weather[0].Description);
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherLastUpdatedAt, DateTime.Now.ToString("ddd" + "," + "hh:mm"));

                    using (var imageGrabClient = new HttpClient())
                    {
                        try
                        {
                            if (weatherRoot.Weather?.Count > 0)
                            {
                                var stream = await imageGrabClient.GetStreamAsync(string.Format(ImageURL, weatherRoot.Weather[0].Icon));
                                Drawable drawable = Drawable.CreateFromStream(stream, StreamSourceName);
                                configurationManager.SaveAValue(ConfigurationParameters.CurrentWeatherIcon, drawable);
                            }
                            else
                            {
                                Console.WriteLine("List of Weather Forecasts is 0 or List is null, won't download any picture.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(Application.Context, $"FAILED TO DOWNLOAD IMAGE {ex}", ToastLength.Long).Show();
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