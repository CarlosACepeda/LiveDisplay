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
                //Trigger an Action to Start LockScreen.
                if (AwakeService.awakeServiceInstance != null)
                {
                    AwakeService.awakeServiceInstance.UnlockScreen();
                }
                
            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                isScreenOn = false;
            }
        }
    }
}