using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.Preferences;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = "android.permission.BIND_DEVICE_ADMIN")]
    [MetaData("android.app.device_admin", Resource = "@xml/device_admin")]
    [IntentFilter(new[] { "android.app.action.DEVICE_ADMIN_ENABLED" })]
    public class AdminReceiver : DeviceAdminReceiver
    {

        public override void OnEnabled(Context context, Intent intent)
        {
            base.OnEnabled(context, intent);
        }

        public override void OnDisabled(Context context, Intent intent)
        {
            base.OnDisabled(context, intent);
        }
    }
}