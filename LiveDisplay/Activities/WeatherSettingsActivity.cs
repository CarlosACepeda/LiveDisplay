namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.App.Job;
    using Android.Content;
    using Android.OS;
    using Android.Telephony;
    using Android.Util;
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using AndroidX.Work;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using LiveDisplay.Servicios.Weather;
    using System;
    using System.Threading;
    using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

    [Activity(Label = "@string/weather", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class WeatherSettingsActivity : AppCompatActivity
    {
        private const int Jobid = 56114281;
        private long interval_minutes;
        private string countryCode = string.Empty;

        private ConfigurationManager configurationManager;
        private AppCompatEditText city;
        private Switch useimperialsystem;
        private Switch allowchangingthewallpaper;
        private TextView citytext;
        private TextView humidity;
        private TextView temperature;
        private TextView minimumTemperature;
        private TextView maximumTemperature;
        private Button saveWeatherPrefs;
        private Spinner weatherupdatefrequency;

        private string units = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.WeatherSettings);
            using (var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
            }
            city = FindViewById<AppCompatEditText>(Resource.Id.cityedit);
            useimperialsystem = FindViewById<Switch>(Resource.Id.useimperialsystem);
            allowchangingthewallpaper = FindViewById<Switch>(Resource.Id.allowchangingthewallpaper);
            useimperialsystem.CheckedChange += Useimperialsystem_CheckedChange;
            allowchangingthewallpaper.CheckedChange += Allowchangingthewallpaper_CheckedChange;

            saveWeatherPrefs = FindViewById<Button>(Resource.Id.savesettings);
            saveWeatherPrefs.Click += Trytogetweather_Click;

            temperature = FindViewById<TextView>(Resource.Id.temperature);
            minimumTemperature = FindViewById<TextView>(Resource.Id.minimumtemperature);
            maximumTemperature = FindViewById<TextView>(Resource.Id.maximumtemperature);
            citytext = FindViewById<TextView>(Resource.Id.city);
            humidity = FindViewById<TextView>(Resource.Id.humidity);
            weatherupdatefrequency = FindViewById<Spinner>(Resource.Id.weatherupdatefrequency);

            var spinnerAdapter = ArrayAdapter<string>.CreateFromResource(this, Resource.Array.listentriesweatherupdatefrequency, Android.Resource.Layout.SimpleSpinnerDropDownItem);

            weatherupdatefrequency.Adapter = spinnerAdapter;
        }

        private void Allowchangingthewallpaper_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            configurationManager.SaveAValue(ConfigurationParameters.WeatherUpdateChangesWallpaper, e.IsChecked);
        }

        protected override void OnResume()
        {
            LoadConfiguration();
            weatherupdatefrequency.ItemSelected += Weatherupdatefrequency_ItemSelected;
            base.OnResume();
        }
        protected override void OnPause()
        {
            weatherupdatefrequency.ItemSelected -= Weatherupdatefrequency_ItemSelected;
            base.OnPause(); 
        }

        private void Weatherupdatefrequency_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            interval_minutes = int.Parse(Resources.GetStringArray(Resource.Array.listvaluesweatherupdatefrequency)[e.Position]);
            configurationManager.SaveAValue(ConfigurationParameters.WeatherUpdateFrequency, e.Position);
        }

        private void Useimperialsystem_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked){
                configurationManager.SaveAValue(ConfigurationParameters.WeatherUseImperialSystem, true);
                units = "imperial";
            }
            else { 
                configurationManager.SaveAValue(ConfigurationParameters.WeatherUseImperialSystem, false);
                units = "metric";
            }
        }
        private void Trytogetweather_Click(object sender, System.EventArgs e)
        {
            if (city?.Text== string.Empty)
            {
                Toast.MakeText(this, "Write the name of your city", ToastLength.Long).Show();
                return;
            }
            if (useimperialsystem.Checked)
            {
                units = MeasurementUnits.Fahrenheit;
            }
            else
            {
                units = MeasurementUnits.Celsius;
            }
            if (interval_minutes != -1)
            {
                PeriodicWorkRequest weatherPeriodicWorkRequest = PeriodicWorkRequest.Builder.From<GrabWeatherJob>(TimeSpan.FromMinutes(interval_minutes)).Build();
                WorkManager.GetInstance(this).Enqueue(weatherPeriodicWorkRequest);

            }
            else
            {
                WorkManager.GetInstance(this).CancelAllWork();
                Toast.MakeText(this, "Set a frequency", ToastLength.Long).Show();
                return;
            }

            saveWeatherPrefs.Enabled = false;


            using (TelephonyManager tm = (TelephonyManager)GetSystemService(TelephonyService))
            {
                countryCode = tm.NetworkCountryIso;
            }

            ThreadPool.QueueUserWorkItem(async m =>
            {
                var weather = await OpenWeatherMapClient.GetWeather(city.Text, countryCode, units);

                RunOnUiThread(() =>
                {
                    temperature.Text = weather?.MainWeather.Temperature.ToString();
                    minimumTemperature.Text = weather?.MainWeather.MinTemperature.ToString();
                    maximumTemperature.Text = weather?.MainWeather.MaxTemperature.ToString();
                    citytext.Text = weather?.Name + ": " + weather?.Weather[0].Description;
                    humidity.Text = Resources.GetString(Resource.String.humidity) + ": " + weather?.MainWeather.Humidity.ToString();

                });
            });
            saveWeatherPrefs.Enabled = true;
        }



        private void LoadConfiguration()
        {
            using (configurationManager = new ConfigurationManager(AppPreferences.Weather))
            {
                city.Text = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, "");
                useimperialsystem.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherUseImperialSystem);
                weatherupdatefrequency.SetSelection(configurationManager.RetrieveAValue(ConfigurationParameters.WeatherUpdateFrequency, 0));
                allowchangingthewallpaper.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherUpdateChangesWallpaper);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            configurationManager.SaveAValue(ConfigurationParameters.WeatherCity, city.Text); //Save before exit, because it might be possible that the EditText never loses focus.

            useimperialsystem.CheckedChange -= Useimperialsystem_CheckedChange;
            city.Dispose();
            useimperialsystem.Dispose();
            configurationManager.Dispose();
        }
    }
}