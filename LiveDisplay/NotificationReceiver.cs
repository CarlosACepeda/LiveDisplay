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

namespace LiveDisplay
{
    [BroadcastReceiver(Enabled = true, Exported =true, Label ="IntentReceiver")]
    [IntentFilter(new[] {"test.test"}, DataMimeType = "text/plain")]
    public class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(Application.Context, "Received intent!", ToastLength.Short).Show();
        }
    }
}