using Android.App;
using Android.Content;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using System.Threading;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "ScreenOnOffReceiver")]
    [IntentFilter(new[] { Intent.ActionScreenOff })]
    [IntentFilter(new[] { Intent.ActionScreenOn })]
    public class ScreenOnOffReceiver : BroadcastReceiver
    {
        public static bool IsScreenOn { get; set; } = true;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionScreenOn)
            {
                //Nice easter eggs here, lol.
                IsScreenOn = true;
            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                //try
                //{
                //    //Java.Lang.Runtime.GetRuntime().Exec("settings put global heads_up_notifications_enabled 0");
                //    string command = "settings put global heads_up_notifications_enabled 0";
                //    Java.Lang.Process p = Java.Lang.Runtime.GetRuntime().Exec(command);
                //    var errorStream = p.ErrorStream;
                //    InputStreamReader inputStreamReader = new InputStreamReader(errorStream);
                //    BufferedReader bf = new BufferedReader(inputStreamReader);
                //    string line;
                //    while((line = bf.ReadLine())!= null)
                //    {
                //        Log.Debug("LiveDisplay", line);
                //    }
                //    //p.WaitFor();
                //}
                //catch (System.Exception)
                //{
                //    throw;
                //}
                //Start hidden in Darkness. :$
                IsScreenOn = false;


                int delaytolockscreen = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.StartLockscreenDelayTime, "0"));

                ThreadPool.QueueUserWorkItem(m =>
                {
                    Thread.Sleep(delaytolockscreen);//Seconds of delay before locking screen(Start the LockScreen Activity)
                                                    //The reason to check if the Screen is turned off is because User can Turn off device screen,
                                                    //then turn it on before the delay to lock screen is finished.
                                                    //So, the Activity will start even if the screen is On, so,
                                                    //in summary the Lockscreen only can start when screen is off
                    using (Intent lockScreenIntent = new Intent(Application.Context, typeof(LockScreenActivity)))
                    {
                        lockScreenIntent.AddFlags(ActivityFlags.NewDocument | ActivityFlags.NoAnimation);

                        if (!IsScreenOn)
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