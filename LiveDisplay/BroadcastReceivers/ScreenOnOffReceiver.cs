using Android.App;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
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
        ConfigurationManager configurationManager = new ConfigurationManager(Application.Context.GetSharedPreferences("livedisplayconfig", FileCreationMode.Private));
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionScreenOn)
            {
                //Toast.MakeText(context, "La pantalla ha sido encendida prro", ToastLength.Short).Show();
                //Nice easter eggs here, lol.
                isScreenOn = true;


            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                //Start hidden in Darkness. :$
                isScreenOn = false;
                //TODO: Add a timer to Start the lockScreen, the timer gets reset when this Intent is triggered.
                //Because sometimes I don't want to Unlock the Lockscreen everytime I turn the screen off.
                //Allowing to the user a more flexible experience. This setting is also configurable
                if (configurationManager.RetrieveAValue(ConfigurationParameters.enabledlockscreennonotifications) == true)
                {
                Intent lockScreenIntent = new Intent(Application.Context, typeof(LockScreenActivity));
                PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, lockScreenIntent, 0);

                pendingIntent.Send();
                }
                
            }
        }
    }
}