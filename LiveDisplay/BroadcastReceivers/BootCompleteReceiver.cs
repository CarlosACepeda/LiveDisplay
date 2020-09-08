using Android.App;
using Android.Content;
using Android.Preferences;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootCompleteReceiver : BroadcastReceiver
    {
        private readonly ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        public override void OnReceive(Context context, Intent intent)
        {
            if (configurationManager.RetrieveAValue(ConfigurationParameters.LockOnBoot))
            {
                Intent lanzarLockScreen = new Intent(context, typeof(LockScreenActivity));
                lanzarLockScreen.AddFlags(ActivityFlags.NewTask);
                PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, lanzarLockScreen, 0);
                pendingIntent.Send();
            }
        }
    }
}