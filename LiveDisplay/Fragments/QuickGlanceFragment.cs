namespace LiveDisplay.Fragments
{
    using Android.App;
    using Android.Content;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Views;
    using Android.Widget;
    using Java.Util;
    using LiveDisplay.BroadcastReceivers;
    using LiveDisplay.Factories;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Notifications;
    using LiveDisplay.Services.Notifications.NotificationEventArgs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fragment = AndroidX.Fragment.App.Fragment;

    public class QuickGlanceFragment : Fragment
    {

        public static event EventHandler ShowMessagesButtonClicked;
        public static event EventHandler WeatherIndicatorClicked;

        private TextView date, battery, messages_counter;
        private ImageView batteryIcon;
        private ImageButton message_indicator;
        private TextView weather_current_degrees;
        private ImageButton weather_indicator;
        private BatteryReceiver batteryReceiver;
        private const int QuickGlanceRequestCode = 256;
        readonly ConfigurationManager configurationManager = new ConfigurationManager();

        //first item indicates if the messaging notification was read
        //second item indicates the notification key to identify which notification was/wasn't read
        private readonly List<Tuple<bool, string>> messagingNotifications = new List<Tuple<bool, string>>();
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        private void SharedPreferenceListenerService_ConfigurationChanged(object sender, Services.Configuration.ConfigurationChangedEventArgs e)
        {
            if (e.Key == ConfigurationParameters.CurrentTemperature)
                weather_current_degrees.Text = (string)e.Value;
            if (e.Key == ConfigurationParameters.CurrentWeatherIcon)
                weather_indicator.SetImageDrawable(configurationManager.RetrieveAValue(e.Key, dummyValue: true));
        }

        private void RegisterBatteryReceiver()
        {
            using IntentFilter intentFilter = new IntentFilter();
            batteryReceiver = new BatteryReceiver();
            intentFilter.AddAction(Intent.ActionBatteryChanged);
            Application.Context.RegisterReceiver(batteryReceiver, intentFilter);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.quick_glance, container, false);
            date = v.FindViewById<TextView>(Resource.Id.date);
            battery = v.FindViewById<TextView>(Resource.Id.batteryLevel);
            batteryIcon = v.FindViewById<ImageView>(Resource.Id.batteryIcon);
            messages_counter = v.FindViewById<TextView>(Resource.Id.messages_counter);
            message_indicator = v.FindViewById<ImageButton>(Resource.Id.message_indicator);

            weather_current_degrees = v.FindViewById<TextView>(Resource.Id.weather_current_degrees);
            weather_indicator = v.FindViewById<ImageButton>(Resource.Id.weather_indicator);
            RegisterBatteryReceiver();

            message_indicator.Click += Message_indicator_Click;
            weather_indicator.Click += Weather_indicator_Click;

            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            CatcherHelper.RequestedOpenNotificationResultGenerated += CatcherHelper_RequestedOpenNotificationResultGenerated;
            NotificationFragment.NotificationRead += NotificationFragment_NotificationRead;
            SharedPreferenceListenerService.ConfigurationChanged += SharedPreferenceListenerService_ConfigurationChanged;

            return v;
        }

        private void Weather_indicator_Click(object sender, EventArgs e)
        {
            WeatherIndicatorClicked?.Invoke(null, null);
        }

        private void NotificationFragment_NotificationRead(object sender, FragmentEventArgs.NotificationReadEventArgs e)
        {
            //Was read by the user
            var item = messagingNotifications.FirstOrDefault(t=> t.Item2 == e.Key);
            if (item != null)
            {
                messagingNotifications?.Remove(item);
                messagingNotifications?.Add(new Tuple<bool, string>(true, e.Key));
            }
            CheckMessagesReadStatusAndDisplayAlert();
        }

        private void CatcherHelper_RequestedOpenNotificationResultGenerated(object sender, RequestedOpenNotificationGeneratedEventArgs e)
        {
            if (e.RequestCode== QuickGlanceRequestCode)
            {
                foreach(var openNotification in e.OpenNotifications)
                {
                    if (!messagingNotifications.Any(n => n.Item2 == openNotification.Key))
                    {
                        messagingNotifications.Add(new Tuple<bool, string>(false, openNotification.Key));
                    }
                }
                MessagesCounterUpdate();
            }
        }

        public override void OnResume()
        {
            LoadDate();
            GetOldMessaggingStyleNotifications();
            GetCurrentWeather();
            base.OnResume();
        }

        private void GetCurrentWeather()
        {
            var currentTemperature = configurationManager.RetrieveAValue(ConfigurationParameters.CurrentTemperature, string.Empty);
            var weatherIcon = configurationManager.RetrieveAValue(ConfigurationParameters.CurrentWeatherIcon, dummyValue: true);

            weather_current_degrees.Text = currentTemperature;
            if (weatherIcon != null)
            {
                weatherIcon = new IconFactory(weatherIcon).ResizeDrawableDp(18, 18).Build();
                weather_indicator.SetImageDrawable(weatherIcon);
            }
        }

        private void GetOldMessaggingStyleNotifications()
        {
            NotificationSlave.GetInstance().RequestOpenNotification(
        on => on.Style == OpenNotification.MessagingStyle && 
        !on.IsSummary, QuickGlanceRequestCode);
        }

        private void Message_indicator_Click(object sender, EventArgs e)
        {
            //Send a beacon lol, asking the notification fragment to show.
            //if(messagingNotifications?.Count>0)
                ShowMessagesButtonClicked?.Invoke(null, null);

        }

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            var item = messagingNotifications.Where(t => t.Item2 == e.OpenNotification.Key).FirstOrDefault();

            if (messagingNotifications.Contains(item))
            {
                messagingNotifications.Remove(item);
                MessagesCounterUpdate();
            }

            CheckMessagesReadStatusAndDisplayAlert();
        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            var item = messagingNotifications.FirstOrDefault(n => n.Item2 == e.OpenNotification.Key);
            if (item!= null)
            {
                messagingNotifications.Remove(item);
            }

            messagingNotifications.Add(new Tuple<bool, string>(false, e.OpenNotification.Key));
            MessagesCounterUpdate();
            CheckMessagesReadStatusAndDisplayAlert();
        }

        public override void OnDestroyView()
        {
            Application.Context.UnregisterReceiver(batteryReceiver);
            BatteryReceiver.BatteryInfoChanged -= BatteryReceiver_BatteryInfoChanged;
            CatcherHelper.RequestedOpenNotificationResultGenerated -= CatcherHelper_RequestedOpenNotificationResultGenerated;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            NotificationFragment.NotificationRead -= NotificationFragment_NotificationRead;
            SharedPreferenceListenerService.ConfigurationChanged -= SharedPreferenceListenerService_ConfigurationChanged;


            base.OnDestroyView();
        }

        private void BatteryReceiver_BatteryInfoChanged(object sender, Services.Battery.BatteryEventArgs.BatteryChangedEventArgs e)
        {
            battery.Text = e.BatteryLevel.ToString() + "%";
            batteryIcon.Background= e.BatteryIcon;
        }

        private void LoadDate()
        {
            using var calendar = Calendar.GetInstance(Locale.Default);
            date.Text = string.Format(calendar.Get(CalendarField.DayOfMonth).ToString() + "/" + calendar.GetDisplayName((int)CalendarField.Month, (int)CalendarStyle.Long, Locale.Default));
        }

        void MessagesCounterUpdate()
        {
            int currentMessagesCount = messagingNotifications.Count;
            if (currentMessagesCount <= 0)
            {
                messages_counter.Text = string.Empty;
                messages_counter.Visibility = ViewStates.Gone;
            }
            else
            {
                messages_counter.Text = currentMessagesCount.ToString();
                messages_counter.Visibility = ViewStates.Visible;
            }
        }
        void CheckMessagesReadStatusAndDisplayAlert()
        {
            if(messagingNotifications!= null && messagingNotifications.Count(m=> m.Item1==false)>0)
            {
                messages_counter.SetBackgroundColor(Android.Graphics.Color.Red);
            }
            else
            {
                messages_counter.SetBackgroundColor(Android.Graphics.Color.Black);
            }
        }
    }
}