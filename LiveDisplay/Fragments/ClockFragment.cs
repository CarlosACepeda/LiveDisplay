namespace LiveDisplay.Fragments
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Provider;
    using Android.Views;
    using Android.Widget;
    using Java.Util;
    using LiveDisplay.BroadcastReceivers;
    using System;
    using Fragment = AndroidX.Fragment.App.Fragment;

    public class ClockFragment : Fragment
    {
        private TextView date;
        private TextClock clock;
        private LinearLayout maincontainer;

        private TextView battery;

        private ImageView batteryIcon;
        private BatteryReceiver batteryReceiver;
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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
            View v = inflater.Inflate(Resource.Layout.cLock2, container, false);
            date = v.FindViewById<TextView>(Resource.Id.date);
            maincontainer = v.FindViewById<LinearLayout>(Resource.Id.container);
            battery = v.FindViewById<TextView>(Resource.Id.batteryLevel);
            batteryIcon = v.FindViewById<ImageView>(Resource.Id.batteryIcon);
            LoadDate();
            RegisterBatteryReceiver();
            maincontainer.Click += MainContainer_Click;
            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;

            return v;
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnPause()
        {
            base.OnPause();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnDestroyView()
        {
            Application.Context.UnregisterReceiver(batteryReceiver);
            maincontainer.Click -= MainContainer_Click;
            BatteryReceiver.BatteryInfoChanged -= BatteryReceiver_BatteryInfoChanged;
            base.OnDestroyView();
        }

        private void BatteryReceiver_BatteryInfoChanged(object sender, Servicios.Battery.BatteryEventArgs.BatteryChangedEventArgs e)
        {
            battery.Text = e.BatteryLevel.ToString() + "%";
            batteryIcon.Background = e.BatteryIcon;
        }

        private void MainContainer_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(AlarmClock.ActionSetAlarm));
        }

        private void LoadDate()
        {
            using var calendar = Calendar.GetInstance(Locale.Default);
            date.Text = string.Format(calendar.Get(CalendarField.DayOfMonth).ToString() + ", " + calendar.GetDisplayName((int)CalendarField.Month, (int)CalendarStyle.Long, Locale.Default));
        }
    }
}