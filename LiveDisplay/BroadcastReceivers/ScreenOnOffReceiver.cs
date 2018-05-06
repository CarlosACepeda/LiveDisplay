using Android.App;
using Android.Content;
using Android.Widget;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "ScreenOnOffReceiver")]
    [IntentFilter(new[] { Intent.ActionScreenOff })]
    [IntentFilter(new[] { Intent.ActionScreenOn })]
    public class ScreenOnOffReceiver : BroadcastReceiver
    {
        public static bool isScreenOn=true;

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionScreenOn)
            {
                Toast.MakeText(context, "La pantalla ha sido encendida prro", ToastLength.Short).Show();
                isScreenOn = true;
            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                isScreenOn = false;
            }
        }
    }
}