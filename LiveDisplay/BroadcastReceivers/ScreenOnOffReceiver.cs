using Android.App;
using Android.Content;
using Android.OS;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;
using System.Threading;

namespace LiveDisplay.BroadcastReceivers
{
    //Android 14 (Api Level 34: Upside Down Cake) made this broadcast receiver useless.
    //As it defers the OnReceive method until my app gets out of the cached state, which means the user must open the app to keep this receiver working accordingly.
    [BroadcastReceiver(Label = "ScreenOnOffReceiver", Enabled =true, Exported = true, Permission = Android.Manifest.Permission.UseFullScreenIntent)]
    [IntentFilter(new[] { Intent.ActionScreenOff })]
    [IntentFilter(new[] { Intent.ActionScreenOn })]
    public class ScreenOnOffReceiver : BroadcastReceiver
    {
        public static bool IsScreenOn { get; set; } = true;
        public static bool ScreenTurnedOffWhileInVertical { get; set; } = true; //most of the times when one turns off the phone the same is vertical.
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
        private NotificationManager notificationManager = null;
        public static int ReceiverCount = 0;

        public override void OnReceive(Context context, Intent intent)
        {

            notificationManager??= (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
            if (intent.Action == Intent.ActionScreenOn)
            {
                //Nice easter eggs here, lol.
                IsScreenOn = true;
            }
            else if (intent.Action == Intent.ActionScreenOff)
            {
                //Start hidden in Darkness. :$
                IsScreenOn = false;

                if (AwakeService.isLaidDown == false)
                {
                    ScreenTurnedOffWhileInVertical = true;
                }
                else 
                {
                    ScreenTurnedOffWhileInVertical = false;
                }

                int delaytolockscreen = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.StartLockscreenDelayTime, "0"));
                Console.WriteLine($"Delay turn off: {delaytolockscreen}");

                ThreadPool.QueueUserWorkItem(m =>
                {
                    Thread.Sleep(delaytolockscreen);//Seconds of delay before locking screen(Start the LockScreen Activity)
                                                    //The reason to check if the Screen is turned off is because User can Turn off device screen,
                                                    //then turn it on before the delay to lock screen is finished.
                                                    //So, the Activity will start even if the screen is On, so,
                                                    //in summary the Lockscreen only can start when screen is off

                    //Workaround for  Android Q (Android 10) devices and above to solve the problem where we can't start activities from foreground or background services
                    //(https://developer.android.com/guide/components/activities/background-starts#display-notification)
                    //what we'll do is to send a Notification that will contain a PendingIntent, this pending intent will start the lockscreen activity.
                    //we set the importance to the Maximum, and set blanks for the title and text, so the user won't notice this notification
                    //It HAS to be set to MAXIMUM importance, any other setting and this won't work.
                    //also we set the Visibility to secret to make this notification even less intrusive to the user.
                    //we set the Max Importance in the SetFullScreenIntent tho, so we have better chances that the device will show the Activity contained
                    //in this FullScreenIntent while the screen is locked for example, cuz apparently this isn't controlled in any way.
                    //the target activity should set the 'setShowWhenLocked(true)' for this to work.
                    //see LockScreenActivity#OnCreate()


                    //Important: To show it on the lockscreen, (in the official documentation, you must click this notification to make the Pending Intent
                    //get sent, thus opening the activity in the PendingIntent), of course this is a lockscreen, so we'll do that in behalf of the user,
                    //for that purpose, I'm using a NotificationListenerService, to recover this notification and click it on behalf of the user
                    //please see: CatcherHelper()#OnNotificationPosted
                    //also your app has to have the "FULL_SCREEN_INTENT" and make use of it.
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    {
                        Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(LockScreenActivity)));

                        PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.Immutable);

                        NotificationChannel notificationChannel = new NotificationChannel("livedisplaynotificationchannel", "LiveDisplay", NotificationImportance.Max);
                        notificationChannel.SetBypassDnd(true);
                        notificationManager.CreateNotificationChannel(notificationChannel);
                        Notification.Builder builder = new Notification.Builder(Application.Context, "livedisplaynotificationchannel");
                        builder.SetContentTitle("");
                        builder.SetContentText("");
                        builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);
                        builder.SetFullScreenIntent(pendingIntent, true);
                        builder.SetVisibility(NotificationVisibility.Secret);

                        notificationManager.Notify(100, builder.Build());
                    }
                    else 
                    {
                        using Intent lockScreenIntent = new Intent(context, typeof(LockScreenActivity));
                        lockScreenIntent.AddFlags(ActivityFlags.NoAnimation);

                        if (IsScreenOn == false)
                        {
                            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, lockScreenIntent, PendingIntentFlags.Immutable);

                            pendingIntent.Send();
                        }
                    }
                });
            }
        }
    }
}