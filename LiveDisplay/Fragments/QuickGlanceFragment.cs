namespace LiveDisplay.Fragments
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Views;
    using Android.Widget;
    using Java.Util;
    using LiveDisplay.BroadcastReceivers;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Notifications;
    using LiveDisplay.Services.Notifications.NotificationEventArgs;
    using System;
    using Fragment = AndroidX.Fragment.App.Fragment;

    public class QuickGlanceFragment : Fragment
    {

        public static event EventHandler ShowMessagesButtonClicked;

        private TextView date, battery, messages_counter;
        private ImageView batteryIcon;
        private ImageButton message_indicator;
        private BatteryReceiver batteryReceiver;
        private int messages_counter_i = 0;
        private const int QuickGlanceRequestCode = 256;
        
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
            messages_counter = v.FindViewById<TextView>(Resource.Id.messages_counter);
            message_indicator = v.FindViewById<ImageButton>(Resource.Id.message_indicator);
            RegisterBatteryReceiver();

            message_indicator.Click += Message_indicator_Click;

            BatteryReceiver.BatteryInfoChanged += BatteryReceiver_BatteryInfoChanged;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            CatcherHelper.RequestedOpenNotificationResultGenerated += CatcherHelper_RequestedOpenNotificationResultGenerated;

            return v;
        }

        private void CatcherHelper_RequestedOpenNotificationResultGenerated(object sender, RequestedOpenNotificationGeneratedEventArgs e)
        {
            if (e.RequestCode== QuickGlanceRequestCode)
            {
                messages_counter_i = e.OpenNotifications.Count;
                messages_counter.Text = messages_counter_i.ToString();
            }
        }

        public override void OnResume()
        {
            LoadDate();
            GetOldMessaggingStyleNotifications();
            base.OnResume();
        }

        private void GetOldMessaggingStyleNotifications()
        {
            NotificationSlave.GetInstance().RequestOpenNotification(
        on => on.Style == OpenNotification.MessagingStyle || (
        Build.VERSION.SdkInt <= BuildVersionCodes.M && on.Style == OpenNotification.InboxStyle), QuickGlanceRequestCode);

        }

        private void Message_indicator_Click(object sender, EventArgs e)
        {
            //Send a beacon lol, asking the notification fragment to show.
            if(messages_counter_i>0)
                ShowMessagesButtonClicked?.Invoke(null, null);

            messages_counter.SetBackgroundColor(Android.Graphics.Color.Black);


        }

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            if (e.OpenNotification.Style == OpenNotification.MessagingStyle || (Build.VERSION.SdkInt <= BuildVersionCodes.M && e.OpenNotification.Style == OpenNotification.InboxStyle))
                messages_counter.Text = ((--messages_counter_i)<0? 0: messages_counter_i).ToString();
        }

        private void CatcherHelper_NotificationPosted(object sender, Services.Notifications.NotificationEventArgs.NotificationPostedEventArgs e)
        {
            if (!e.UpdatesPreviousNotification)
                if (e.OpenNotification.Style == OpenNotification.MessagingStyle || (Build.VERSION.SdkInt <= BuildVersionCodes.M && e.OpenNotification.Style == OpenNotification.InboxStyle))
                {
                    messages_counter.Text = (++messages_counter_i).ToString();
                    messages_counter.SetBackgroundColor(Android.Graphics.Color.Red);
                }
        }




        public override void OnDestroyView()
        {
            Application.Context.UnregisterReceiver(batteryReceiver);
            BatteryReceiver.BatteryInfoChanged -= BatteryReceiver_BatteryInfoChanged;
            CatcherHelper.RequestedOpenNotificationResultGenerated -= CatcherHelper_RequestedOpenNotificationResultGenerated;
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
    }
}