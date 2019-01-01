using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.DataRepository;
using LiveDisplay.Misc;
using System;

namespace LiveDisplay.Fragments
{
    public class ClockFragment : Fragment
    {
        private TextView date;
        private TextClock clock;
        private TextView battery;
        private ImageView batteryIcon;
        private BatteryReceiver batteryReceiver;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RegisterBatteryReceiver();


            // Create your fragment here

            // TODO: Implement me

            //if (sharedPreferences.GetBoolean(ConfigurationParameters.hiddenclock, false) == true)
            //{
            //    //Hide the clock
            //    RunOnUiThread(() => reloj.Visibility = ViewStates.Gone);
            //}

            //TODO: Implement me too. ;)
            //if (configurationManager.RetrieveAValue(ConfigurationParameters.hiddensystemicons) == true)
            //{
            //}
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
            battery = v.FindViewById<TextView>(Resource.Id.batteryLevel);
            batteryIcon = v.FindViewById<ImageView>(Resource.Id.batteryIcon);
            LoadDate();

            //View Events
            clock.Click += Clock_Click;
            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;

            return v;
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