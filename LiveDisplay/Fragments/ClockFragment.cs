namespace LiveDisplay.Fragments
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Preferences;
    using Android.Provider;
    using Android.Views;
    using Android.Widget;
    using Java.Util;
    using LiveDisplay.BroadcastReceivers;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using System;


    public class ClockFragment : Fragment
    {
        private readonly ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Weather);
        private TextView date;
        private TextClock clock;
        //private RelativeLayout weatherclockcontainer;
        private TextView battery;
        private ImageView batteryIcon;
        private BatteryReceiver batteryReceiver;
        private TextView temperature;
        //private TextView minimumTemperature;
        //private TextView maximumTemperature;
        //private TextView humidity;
        private TextView description;
        //private TextView lastupdated;
        //private TextView city;
        //private LinearLayout weatherinfo;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RegisterBatteryReceiver();
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
            View v = inflater.Inflate(Resource.Layout.cLock, container, false);
            date = v.FindViewById<TextView>(Resource.Id.txtFechaLock);
            clock = v.FindViewById<TextClock>(Resource.Id.clockLock);
            //weatherclockcontainer = v.FindViewById<RelativeLayout>(Resource.Id.weatherclockcontainer);
            battery = v.FindViewById<TextView>(Resource.Id.batteryLevel);
            batteryIcon = v.FindViewById<ImageView>(Resource.Id.batteryIcon);
            temperature = v.FindViewById<TextView>(Resource.Id.temperature);
            //minimumTemperature = v.FindViewById<TextView>(Resource.Id.minimumtemperature);
            //maximumTemperature = v.FindViewById<TextView>(Resource.Id.maximumtemperature);
            //humidity = v.FindViewById<TextView>(Resource.Id.humidity);
            description = v.FindViewById<TextView>(Resource.Id.weatherdescription);
            //lastupdated = v.FindViewById<TextView>(Resource.Id.lastupdated);
            //city = v.FindViewById<TextView>(Resource.Id.city);
            LoadDate();

            //View Events
            clock.Click += Clock_Click;
            //weatherclockcontainer.Click += Weatherclockcontainer_Click;
            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

            if (configurationManager.RetrieveAValue(ConfigurationParameters.HiddenClock) == true)
            {
                //Hide the clock
                Activity.RunOnUiThread(() => clock.Visibility = ViewStates.Invisible);
            }
            return v;
        }
        public override void OnResume()
        {
            LoadWeather();
            base.OnResume();    
        }

        private void LoadWeather()
        {
            string thecity = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCity, string.Empty);
            string currentweather = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherCurrent, string.Empty);
            string minimumtemperature = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherMinimum, string.Empty);
            string maximumtemperature = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherMaximum, string.Empty);
            string weatherhumidity = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherHumidity, string.Empty);
            string weatherdescription = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherDescription, string.Empty);
            string weatherlastupdated = configurationManager.RetrieveAValue(ConfigurationParameters.WeatherLastUpdated, string.Empty);

            temperature.Text = currentweather;
            //city.Text = thecity;
            //minimumTemperature.Text = minimumtemperature;
            //maximumTemperature.Text = maximumtemperature;
            //humidity.Text = weatherhumidity;
            description.Text = weatherdescription;
            //lastupdated.Text = weatherlastupdated;
        }

        private void Weatherclockcontainer_Click(object sender, EventArgs e)
        {
            var view = sender as View;
            //weatherinfo = view.FindViewById<LinearLayout>(Resource.Id.weatherinfo);
            //if (weatherinfo.Visibility == ViewStates.Visible)
            //{
            //    weatherinfo.Visibility = ViewStates.Invisible;
            //}
            //else
            //{
            //    weatherinfo.Visibility = ViewStates.Visible;
            //}
            //weatherinfo.Dispose();
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            battery.Dispose();
            Application.Context.UnregisterReceiver(batteryReceiver);
            BatteryReceiver.BatteryInfoChanged -= BatteryReceiver_BatteryInfoChanged;
        }

        private void BatteryReceiver_BatteryInfoChanged(object sender, Servicios.Battery.BatteryEventArgs.BatteryChangedEventArgs e)
        {
            battery.Text = e.BatteryLevel.ToString() + "%";
            batteryIcon.Background = e.BatteryIcon;
        }

        private void Clock_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent(AlarmClock.ActionShowAlarms))
            {
                StartActivity(intent);
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
    }
}