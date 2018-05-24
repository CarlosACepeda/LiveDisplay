using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using LiveDisplay.Servicios;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "ScreenOnOffReceiver", Name = "Underground.LiveDisplay.BroadcastReceivers.ScreenOnOffReceiver")]
    [IntentFilter(new[] { Intent.ActionScreenOff })]
    [IntentFilter(new[] { Intent.ActionScreenOn })]
    public class ScreenOnOffReceiver : BroadcastReceiver
    {
        public static bool isScreenOn=true;

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionScreenOn )
            {
                Toast.MakeText(context, "La pantalla ha sido encendida prro", ToastLength.Short).Show();
                isScreenOn = true;
                //Trigger an Action to Start LockScreen when enabled, by default it is.
                Intent lockScreenIntent = new Intent(Application.Context, typeof(LockScreenActivity));
                lockScreenIntent.AddFlags(ActivityFlags.NewTask);
                
                Application.Context.StartActivity(lockScreenIntent);
                
            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                isScreenOn = false;
            }
        }
    }
}