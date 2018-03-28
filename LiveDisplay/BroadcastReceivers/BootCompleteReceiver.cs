using Android.App;
using Android.Content;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = "android.permission.RECEIVE_BOOT_COMPLETED", Label = "BootBroadcastReceiver")]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootCompleteReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //funciona, no se hace nada más aquí
            Intent lanzarLockScreen = new Intent(context, typeof(LockScreenActivity));
            //Tener en cuenta, posible Memory Leak aquí
            lanzarLockScreen.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(lanzarLockScreen);
        }
    }
}