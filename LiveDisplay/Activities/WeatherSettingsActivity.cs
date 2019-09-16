namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.App.Job;
    using Android.Content;
    using Android.OS;
    using Android.Support.V7.App;
    using Android.Telephony;
    using Android.Util;
    using Android.Views;
    using Android.Widget;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using LiveDisplay.Servicios.Weather;
    using System;
    using System.Threading;

    [Activity(Label = "@string/weather", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
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
        private Spinner weatherupdatefrequency;

        private string units = "";

        private string currentcity = "";

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
            weatherupdatefrequency = FindViewById<Spinner>(Resource.Id.weatherupdatefrequency);

            var spinnerAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.listentriesweatherupdatefrequency, Android.Resource.Layout.SimpleSpinnerDropDownItem);

            weatherupdatefrequency.Adapter = spinnerAdapter;
            weatherupdatefrequency.ItemSelected += Weatherupdatefrequency_ItemSelected; 
            LoadConfiguration();
        }

        private void Weatherupdatefrequency_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //TODO.
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

                ;
                JobInfo.Builder jobBuilder = new JobInfo.Builder(56114281, new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(GrabWeatherJob))));
                jobBuilder.SetPersisted(true);
                jobBuilder.SetPeriodic(1000 * 60 * 15); //15 Minutes.
                JobScheduler jobScheduler = (JobScheduler)Application.Context.GetSystemService(Context.JobSchedulerService);
                int result = jobScheduler.Schedule(jobBuilder.Build());
                if (result == JobScheduler.ResultSuccess)
                {
                    Log.Info("LiveDisplay", "Job Result Sucess");
                }
                else
                {
                    Log.Info("LiveDisplay", "Job Result Not Sucess");
                }
            });
        }

        private void Useimperialsystem_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            switch (e.IsChecked)
            {
                case true:
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherUseImperialSystem, true);
                    units = "imperial";
                    break;

                case false:
                    configurationManager.SaveAValue(ConfigurationParameters.WeatherUseImperialSystem, false);
                    units = "metric";
                    break;

                default:
                    break;
            }
        }

        private void City_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus == false)
            {
                configurationManager.SaveAValue(ConfigurationParameters.WeatherCity, city.Text);
            }
            currentcity = city.Text;
        }

        private void LoadConfiguration()
        {
            using (configurationManager = new ConfigurationManager(sharedPreferences))
            {
                currentcity = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, "");
                city.Text = currentcity;
                useimperialsystem.Checked =
                    configurationManager.RetrieveAValue(ConfigurationParameters.WeatherUseImperialSystem);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            configurationManager.SaveAValue(ConfigurationParameters.WeatherCity, city.Text); //Save before exit, because it might be possible that the EditText never loses focus.

            city.FocusChange -= City_FocusChange;
            useimperialsystem.CheckedChange -= Useimperialsystem_CheckedChange;
            city.Dispose();
            useimperialsystem.Dispose();
            configurationManager.Dispose();
        }
    }
}