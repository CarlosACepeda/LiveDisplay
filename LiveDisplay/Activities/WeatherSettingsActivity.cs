using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Weather;
using System.Threading;

namespace LiveDisplay.Activities
{
    [Activity(Label = "Weather", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class WeatherSettingsActivity : AppCompatActivity
    {
        private ConfigurationManager configurationManager;
        private ISharedPreferences sharedPreferences;
        private EditText city;
        private Switch useimperialsystem;
        private TextView citytext;
        private TextView humidity;
        private TextView temperature;
        private TextView minimumTemperature;
        private TextView maximumTemperature;
        private Button trytogetweather;

        private string currentcity = "";
        private UnitsFlags units;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            sharedPreferences = Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private);

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.WeatherSettings);
            using (var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
            }
            city = FindViewById<EditText>(Resource.Id.cityedit);
            useimperialsystem = FindViewById<Switch>(Resource.Id.useimperialsystem);
            city.FocusChange += City_FocusChange;
            useimperialsystem.CheckedChange += Useimperialsystem_CheckedChange;

            trytogetweather = FindViewById<Button>(Resource.Id.trytogetweather);
            trytogetweather.Click += Trytogetweather_Click;


            temperature = FindViewById<TextView>(Resource.Id.temperature);
            minimumTemperature = FindViewById<TextView>(Resource.Id.minimumtemperature);
            maximumTemperature = FindViewById<TextView>(Resource.Id.maximumtemperature);
            citytext = FindViewById<TextView>(Resource.Id.city);
            humidity = FindViewById<TextView>(Resource.Id.humidity);
            

            LoadConfiguration();
        }

        private void Trytogetweather_Click(object sender, System.EventArgs e)
        {
            trytogetweather.Text = "wait...";
            trytogetweather.Enabled = false;

            string countryCode = "";
            string temperatureSuffix = "°c";
            using (TelephonyManager tm = (TelephonyManager)GetSystemService(Context.TelephonyService))
            {
                countryCode = tm.NetworkCountryIso;
            }
            if (units == UnitsFlags.Imperial)
            {
                temperatureSuffix = "°f";
            }

            

            ThreadPool.QueueUserWorkItem(async m =>
            {
                var weather = await Weather.GetWeather(currentcity, countryCode, units);
                RunOnUiThread(() =>
                {
                    temperature.Text = weather?.MainWeather.Temperature.ToString() + temperatureSuffix;
                    minimumTemperature.Text = "min: " + weather?.MainWeather.MinTemperature.ToString() + temperatureSuffix;
                    maximumTemperature.Text = "max: " + weather?.MainWeather.MaxTemperature.ToString() + temperatureSuffix;
                    citytext.Text = weather?.Name + ": " + weather?.Weather[0].Description;
                    humidity.Text = Resources.GetString(Resource.String.humidity) + ": " + weather?.MainWeather.Humidity.ToString();

                    trytogetweather.Text = "Test me";
                    trytogetweather.Enabled = true;
                });
            });
        }

        private void Useimperialsystem_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            switch (e.IsChecked)
            {
                case true:
                    configurationManager.SaveAValue(ConfigurationParameters.UseImperialSystem, true);
                    break;

                case false:
                    configurationManager.SaveAValue(ConfigurationParameters.UseImperialSystem, false);
                    break;

                default:
            }
        }

        private void City_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus == false)
            {
                configurationManager.SaveAValue(ConfigurationParameters.City, city.Text);
            }
            currentcity = city.Text;
        }

        private void LoadConfiguration()
        {
            using (configurationManager = new ConfigurationManager(sharedPreferences))
            {
                currentcity = configurationManager.RetrieveAValue(ConfigurationParameters.City, "");
                city.Text = currentcity;
                useimperialsystem.Checked =
                    configurationManager.RetrieveAValue(ConfigurationParameters.UseImperialSystem);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            configurationManager.SaveAValue(ConfigurationParameters.City, city.Text); //Save before exit, because it might be possible that the EditText never loses focus.

            city.FocusChange -= City_FocusChange;
            useimperialsystem.CheckedChange -= Useimperialsystem_CheckedChange;
            city.Dispose();
            useimperialsystem.Dispose();
            configurationManager.Dispose();
        }
    }
}