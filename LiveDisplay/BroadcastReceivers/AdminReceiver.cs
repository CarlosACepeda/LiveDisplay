using Android.App;
using Android.App.Admin;
using Android.Content;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = "android.permission.BIND_DEVICE_ADMIN")]
    [MetaData("android.app.device_admin", Resource = "@xml/device_admin")]
    [IntentFilter(new[] { "android.app.action.DEVICE_ADMIN_ENABLED" })]
    public class AdminReceiver : DeviceAdminReceiver
    {
        public static bool isAdminGiven = false;

        public override void OnEnabled(Context context, Intent intent)
        {
            isAdminGiven = true;
            Console.WriteLine("Admin Given");
            base.OnEnabled(context, intent);
        }

        public override void OnDisabled(Context context, Intent intent)
        {
            isAdminGiven = false;
            Console.WriteLine("Admin Disabled");
            base.OnDisabled(context, intent);
        }
    }
}