using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.DataRepository;
using LiveDisplay.Factories;
using LiveDisplay.Misc;

namespace LiveDisplay.Fragments
{
    public class ClockFragment : Fragment
    {
        private TextView date;
        private TextClock clock;
        private TextView battery;
        private ImageView batteryIcon;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.cLock, container, false);
            date = v.FindViewById<TextView>(Resource.Id.txtFechaLock);
            clock = v.FindViewById<TextClock>(Resource.Id.clockLock);
            battery = v.FindViewById<TextView>(Resource.Id.batteryLevel);
            batteryIcon = v.FindViewById<ImageView>(Resource.Id.batteryIcon);
            LoadDate();

            battery.Text = Battery.ReturnBatteryLevel().ToString() + "%";
            SetBackgroundAccordingWithBatteryLevel(Battery.BatteryLevelFlags);

            //View Events
            clock.Click += Clock_Click;
            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;

            return v;


        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            battery.Dispose();
            BatteryReceiver.BatteryInfoChanged-= BatteryReceiver_BatteryInfoChanged;
        }
        private void BatteryReceiver_BatteryInfoChanged(object sender, Servicios.Battery.BatteryEventArgs.BatteryChangedEventArgs e)
        {
            battery.Text = e.BatteryLevel.ToString() + "%";
            SetBackgroundAccordingWithBatteryLevel(e);

        }

        private void SetBackgroundAccordingWithBatteryLevel(Servicios.Battery.BatteryEventArgs.BatteryChangedEventArgs e)
        {
            switch (e.BatteryLevelFlags)
            {
                case BatteryLevelFlags.OverZero:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.battery_alert_white_18dp);
                    break;
                case BatteryLevelFlags.OverTwenty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_20_white_18);
                    break;
                case BatteryLevelFlags.OverThirty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_30_white_18);
                    break;
                case BatteryLevelFlags.OverFifty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_50_white_18);
                    break;
                case BatteryLevelFlags.OverSixty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_60_white_18);
                    break;
                case BatteryLevelFlags.OverEighty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_80_white_18);
                    break;
                case BatteryLevelFlags.OverNinety:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_90_white_18);
                    break;
                default:
                    break;
            }
        }
        private void SetBackgroundAccordingWithBatteryLevel(BatteryLevelFlags batteryLevelFlags)
        {
            switch (batteryLevelFlags)
            {
                case BatteryLevelFlags.OverZero:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.battery_alert_white_18dp);
                    break;
                case BatteryLevelFlags.OverTwenty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_20_white_18);
                    break;
                case BatteryLevelFlags.OverThirty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_30_white_18);
                    break;
                case BatteryLevelFlags.OverFifty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_50_white_18);
                    break;
                case BatteryLevelFlags.OverSixty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_60_white_18);
                    break;
                case BatteryLevelFlags.OverEighty:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_80_white_18);
                    break;
                case BatteryLevelFlags.OverNinety:
                    batteryIcon.SetBackgroundResource(Resource.Drawable.baseline_battery_90_white_18);
                    break;
                default:
                    break;
            }
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