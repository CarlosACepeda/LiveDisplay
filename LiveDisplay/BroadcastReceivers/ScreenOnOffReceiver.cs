using Android.App;
using Android.Content;
using Android.Preferences;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System.Threading;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "ScreenOnOffReceiver")]
    [IntentFilter(new[] { Intent.ActionScreenOff })]
    [IntentFilter(new[] { Intent.ActionScreenOn })]
    public class ScreenOnOffReceiver : BroadcastReceiver
    {
        public static bool isScreenOn = true;
        private ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context));
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionScreenOn)
            {
                //Nice easter eggs here, lol.
                isScreenOn = true;
            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                //Start hidden in Darkness. :$
                isScreenOn = false;
                    int delaytolockscreen = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.startlockscreendelaytime, "0"));

                    ThreadPool.QueueUserWorkItem(m =>
                    {
                            Thread.Sleep(delaytolockscreen);//Seconds of delay before locking screen(Start the LockScreen Activity)
                                                            //The reason to check if the Screen is turned off is because User can Turn off device screen,
                                                            //then turn it on before the delay to lock screen is finished.
                                                            //So, the Activity will start even if the screen is On, so,
                                                            //in summary the Lockscreen only can start when screen is off
                            using (Intent lockScreenIntent = new Intent(Application.Context, typeof(LockScreenActivity)))
                            {
                                if (isScreenOn == false)
                                {
                                    PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, lockScreenIntent, 0);

                                    pendingIntent.Send();
                                }
                            }

                        
                    });
                
            }
        }
    }
}