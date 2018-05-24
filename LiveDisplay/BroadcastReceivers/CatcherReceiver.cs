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
    [BroadcastReceiver(Label = "CatcherReceiver", Name = "Underground.LiveDisplay.BroadcastReceivers.CatcherReceiver")]
    [IntentFilter(new[] { "CatcherIntent"})]
    public class CatcherReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //TODO: TRANSPORT NOTIFICATION TO HERE
            Toast.MakeText(context, "Received intent! from Catcher", ToastLength.Short).Show();
        }
    }
}