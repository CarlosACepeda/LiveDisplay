using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;
using System.Threading;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "ScreenOnOffReceiver")]
    [IntentFilter(new[] { Intent.ActionScreenOff })]
    [IntentFilter(new[] { Intent.ActionScreenOn })]
    public class ScreenOnOffReceiver : BroadcastReceiver
    {
        public static bool isScreenOn=true;       
        ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context));
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
                    int delaytolockscreen = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.startlockscreendelaytime, "0"));
                    

                    ThreadPool.QueueUserWorkItem(m =>
                        {
                                                     
                            Thread.Sleep(delaytolockscreen);//Seconds of delay before locking screen(Start the LockScreen Activity)
                            //The reason to check if the Screen is turned off is because User can Turn off device screen,
                            //then turn it on before the delay to lock screen is finished. 
                            //So, the Activity will start even if the screen is On, so,
                            //in summary the Lockscreen only can start when screen is off
                            if (isScreenOn == false)
                            {
                                Intent lockScreenIntent = new Intent(Application.Context, typeof(LockScreenActivity));
                                PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, lockScreenIntent, 0);

                                pendingIntent.Send();
                            }
                        });
                    
                }
                
            }
        }
    }
}