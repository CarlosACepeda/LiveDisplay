using Android.App;
using Android.Content;
using LiveDisplay.Misc;
using LiveDisplay.Services;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = "android.permission.RECEIVE_BOOT_COMPLETED", Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootCompleteReceiver : BroadcastReceiver
    {
        private readonly ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        public override void OnReceive(Context context, Intent intent)
        {
            if (configurationManager.RetrieveAValue(ConfigurationParameters.LockOnBoot))
            {
                Intent lockscreenLaunch = new Intent(context, typeof(LockScreenActivity));
                lockscreenLaunch.AddFlags(ActivityFlags.NoAnimation);
                PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, lockscreenLaunch, PendingIntentFlags.Immutable);
                pendingIntent.Send();
            }
        }
    }
}