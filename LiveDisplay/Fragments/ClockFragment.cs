namespace LiveDisplay.Fragments
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Provider;
    using Android.Util;
    using Android.Views;
    using Android.Widget;
    using Java.Util;
    using LiveDisplay.BroadcastReceivers;
    using LiveDisplay.Enums;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Keyguard;
    using LiveDisplay.Services.Weather;
    using LiveDisplay.Services.Widget;
    using System;
    using Fragment = AndroidX.Fragment.App.Fragment;

    public class ClockFragment : Fragment
    {
        private readonly ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Weather);
        private TextView date;
        private TextClock clock;
        private LinearLayout maincontainer;

        //private RelativeLayout weatherclockcontainer;
        private TextView battery;

        private ImageView batteryIcon;
        private BatteryReceiver batteryReceiver;
        private TextView temperature;

        //private TextView minimumTemperature;
        //private TextView maximumTemperature;
        //private TextView humidity;
        private TextView description;

        private TextView lastupdated;
        private TextView city;

        //private LinearLayout weatherinfo;
        private bool initForFirstTime = false;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            WidgetStatusPublisher.GetInstance().OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;
        }

        private void RegisterBatteryReceiver()
        {
            using (IntentFilter intentFilter = new IntentFilter())
            {
                batteryReceiver = new BatteryReceiver();
                intentFilter.AddAction(Intent.ActionBatteryChanged);
                Application.Context.RegisterReceiver(batteryReceiver, intentFilter);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.cLock2, container, false);
            date = v.FindViewById<TextView>(Resource.Id.txtFechaLock);
            clock = v.FindViewById<TextClock>(Resource.Id.clockLock);
            maincontainer = v.FindViewById<LinearLayout>(Resource.Id.container);
            battery = v.FindViewById<TextView>(Resource.Id.batteryLevel);
            batteryIcon = v.FindViewById<ImageView>(Resource.Id.batteryIcon);
            temperature = v.FindViewById<TextView>(Resource.Id.temperature);
            //minimumTemperature = v.FindViewById<TextView>(Resource.Id.minimumtemperature);
            //maximumTemperature = v.FindViewById<TextView>(Resource.Id.maximumtemperature);
            //humidity = v.FindViewById<TextView>(Resource.Id.humidity);
            description = v.FindViewById<TextView>(Resource.Id.weatherdescription);
            lastupdated = v.FindViewById<TextView>(Resource.Id.lastupdated);
            city = v.FindViewById<TextView>(Resource.Id.city);
            LoadDate();
            RegisterBatteryReceiver();

            //View Events
            clock.Click += Clock_Click;
            //weatherclockcontainer.Click += Weatherclockcontainer_Click;
            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;

            return v;
        }

        public override void OnResume()
        {
            LoadWeather();
            GrabWeatherJob.WeatherUpdated += GrabWeatherJob_WeatherUpdated;

            base.OnResume();
        }

        private void WidgetStatusPublisher_OnWidgetStatusChanged(object sender, WidgetStatusEventArgs e)
        {
            if (e.WidgetName == WidgetTypes.CLOCK_FRAGMENT)
            {
                if (e.Show)
                {
                    ToggleWidgetVisibility(true);
                }
                else
                {
                    ToggleWidgetVisibility(false);
                }
            }
        }
        private void ToggleWidgetVisibility(bool visible)
        {
            Activity.RunOnUiThread(() => {
                if (maincontainer != null)
                {
                    if (visible)
                    {
                        maincontainer.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        maincontainer.Visibility = ViewStates.Invisible;
                    }
                }

            });
        }

        public override void OnPause()
        {
            GrabWeatherJob.WeatherUpdated -= GrabWeatherJob_WeatherUpdated;

            base.OnPause();
        }

        public override void OnDestroy()
        {
            WidgetStatusPublisher.GetInstance().OnWidgetStatusChanged -= WidgetStatusPublisher_OnWidgetStatusChanged;
            ToggleWidgetVisibility(false);
            initForFirstTime = false;
            base.OnDestroy();
        }

        private void GrabWeatherJob_WeatherUpdated(object sender, bool e)
        {
            LoadWeather();
        }

        private void LoadWeather()
        {
            string thecity = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, string.Empty);
            string currentweather = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCurrent, string.Empty);
            string temperatureunit = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherTemperatureUnit, string.Empty);
            string minimumtemperature = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherMinimum, string.Empty);
            string maximumtemperature = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherMaximum, string.Empty);
            string weatherhumidity = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherHumidity, string.Empty);
            string weatherdescription = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherDescription, string.Empty);
            string weatherlastupdated = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherLastUpdated, string.Empty);

            Activity.RunOnUiThread(() =>
            {
                temperature.Text = currentweather + temperatureunit;
                city.Text = thecity;
                //minimumTemperature.Text = minimumtemperature;
                //maximumTemperature.Text = maximumtemperature;
                //humidity.Text = weatherhumidity;
                description.Text = weatherdescription;
                lastupdated.Text = weatherlastupdated;
            });
        }
        public override void OnDestroyView()
        {
            base.OnDestroyView();

            battery.Dispose();
            Application.Context.UnregisterReceiver(batteryReceiver);
            clock.Click -= Clock_Click;
            BatteryReceiver.BatteryInfoChanged -= BatteryReceiver_BatteryInfoChanged;
        }

        private void BatteryReceiver_BatteryInfoChanged(object sender, Services.Battery.BatteryEventArgs.BatteryChangedEventArgs e)
        {
            battery.Text = e.BatteryLevel.ToString() + "%";
            batteryIcon.Background = e.BatteryIcon;
        }

        private void Clock_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent(AlarmClock.ActionShowAlarms))
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    if (KeyguardHelper.IsDeviceCurrentlyLocked())
                    {
                        KeyguardHelper.RequestDismissKeyguard(Activity);
                    }
                try
                {
                    StartActivity(intent);
                }
                catch(AndroidRuntimeException arex)
                {
                    Log.Warn("TAG", arex);
                    clock.Click -= Clock_Click;
                }
            }
        }

        private void LoadDate()
        {
            //Load date
            using (var calendar = Calendar.GetInstance(Locale.Default))
            {
                date.Text = string.Format(calendar.Get(CalendarField.DayOfMonth).ToString() + ", " + calendar.GetDisplayName((int)CalendarField.Month, (int)CalendarStyle.Long, Locale.Default));
            }
        }
        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean("FRAGMENT", true);
            base.OnSaveInstanceState(outState);
        }
        public override void OnViewStateRestored(Bundle savedInstanceState)
        {
            var value= savedInstanceState?.GetBoolean("FRAGMENT");

            Console.WriteLine("Clock says: " + value);
            base.OnViewStateRestored(savedInstanceState);
        }
    }
}