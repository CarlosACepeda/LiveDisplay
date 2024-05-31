namespace LiveDisplay.Fragments
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Views;
    using Android.Widget;
    using Java.Util;
    using LiveDisplay.BroadcastReceivers;
    using Fragment = AndroidX.Fragment.App.Fragment;

    public class QuickGlanceFragment : Fragment
    {
        private TextView date, battery;
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
            View v = inflater.Inflate(Resource.Layout.quick_glance, container, false);
            date = v.FindViewById<TextView>(Resource.Id.date);
            battery = v.FindViewById<TextView>(Resource.Id.batteryLevel);
            batteryIcon = v.FindViewById<ImageView>(Resource.Id.batteryIcon);
            LoadDate();
            RegisterBatteryReceiver();
            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;

            return v;
        }
        public override void OnDestroyView()
        {
            Application.Context.UnregisterReceiver(batteryReceiver);
            BatteryReceiver.BatteryInfoChanged -= BatteryReceiver_BatteryInfoChanged;
            base.OnDestroyView();
        }

        private void BatteryReceiver_BatteryInfoChanged(object sender, Services.Battery.BatteryEventArgs.BatteryChangedEventArgs e)
        {
            battery.Text = e.BatteryLevel.ToString() + "%";
            batteryIcon.SetBackgroundDrawable(e.BatteryIcon);
        }

        private void LoadDate()
        {
            using var calendar = Calendar.GetInstance(Locale.Default);
            date.Text = string.Format(calendar.Get(CalendarField.DayOfMonth).ToString() + "/" + calendar.GetDisplayName((int)CalendarField.Month, (int)CalendarStyle.Long, Locale.Default));
        }
    }
}