using Android.App;
using Android.Content;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = Android.Manifest.Permission.ReceiveBootCompleted, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootCompleteReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
          //Change behavior on boot.
        }
    }
}