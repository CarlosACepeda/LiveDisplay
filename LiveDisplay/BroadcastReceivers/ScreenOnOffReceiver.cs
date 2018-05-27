using Android.App;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios;
using System;

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
            if (intent.Action == Intent.ActionScreenOn )
            {
                //Toast.MakeText(context, "La pantalla ha sido encendida prro", ToastLength.Short).Show();
                //Nice easter eggs here, lol.
                isScreenOn = true;
               

            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                //Start hidden in Darkness. :$
                isScreenOn = false;
                //Trigger an Action to Start LockScreen when enabled, by default it is.
                Intent lockScreenIntent = new Intent(Application.Context, typeof(LockScreenActivity));
                lockScreenIntent.AddFlags(ActivityFlags.NewTask);
                
                PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, lockScreenIntent, 0);

                pendingIntent.Send();
            }
        }
    }
}