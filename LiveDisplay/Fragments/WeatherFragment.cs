using Android.Content;
using Android.OS;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Weather;
using System.Threading;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class WeatherFragment : Fragment
    {
        private ConfigurationManager configurationManager;

        private TextView temperature;
        private TextView minimumTemperature;
        private TextView maximumTemperature;
        private TextView city;
        private TextView humidity;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            configurationManager = new ConfigurationManager(AppPreferences.Weather);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.Weather, container, false);
            temperature = v.FindViewById<TextView>(Resource.Id.temperature);
            minimumTemperature = v.FindViewById<TextView>(Resource.Id.minimumtemperature);
            maximumTemperature = v.FindViewById<TextView>(Resource.Id.maximumtemperature);
            city = v.FindViewById<TextView>(Resource.Id.city);
            humidity = v.FindViewById<TextView>(Resource.Id.humidity);
            TelephonyManager tm = (TelephonyManager)Activity.GetSystemService(Context.TelephonyService);
            string countryCode = tm.NetworkCountryIso;

            string thecity = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, "");
            string units = "metric";
            string temperatureSuffix = "°C";
            if (configurationManager.RetrieveAValue(ConfigurationParameters.WeatherUseImperialSystem) == true)
            {
                units = "imperial";
                temperatureSuffix = "°F";
            }

            ThreadPool.QueueUserWorkItem(async m =>
            {
                var weather = await OpenWeatherMapClient.GetWeather(thecity, countryCode, units);
                Activity.RunOnUiThread(() =>
                {
                    temperature.Text = weather?.MainWeather.Temperature.ToString() + temperatureSuffix;
                    minimumTemperature.Text = "min: " + weather?.MainWeather.MinTemperature.ToString() + temperatureSuffix;
                    maximumTemperature.Text = "max: " + weather?.MainWeather.MaxTemperature.ToString() + temperatureSuffix;
                    city.Text = weather?.Name + ": " + weather?.Weather[0].Description;
                    humidity.Text = Resources.GetString(Resource.String.humidity) + ": " + weather?.MainWeather.Humidity.ToString();
                });
            });
            return v;
        }
    }
}