using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label ="Battery Receiver")]
    [IntentFilter(new[] {Intent.ActionBatteryChanged })]
    public class BatteryReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            int batterylevel = intent.GetIntExtra(BatteryManager.ExtraLevel, 0);
            Toast.MakeText(context, "Battery level is "+batterylevel, ToastLength.Short).Show();
        }
    }
}