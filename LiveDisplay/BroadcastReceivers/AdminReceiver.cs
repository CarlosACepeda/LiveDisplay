using Android;
using Android.App;
using Android.App.Admin;
using Android.Content;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = Manifest.Permission.BindDeviceAdmin, Exported =true)]
    [MetaData( DeviceAdminMetaData, Resource = "@xml/device_admin")]
    [IntentFilter(new[] {ActionDeviceAdminEnabled })]
    public class AdminReceiver : DeviceAdminReceiver
    {
        public static event EventHandler<bool> OnDeviceAdminEnabled;
        public override void OnEnabled(Context context, Intent intent)
        {
            base.OnEnabled(context, intent);
            OnDeviceAdminEnabled?.Invoke(this, true);
        }

        public override void OnDisabled(Context context, Intent intent)
        {
            base.OnDisabled(context, intent);
            OnDeviceAdminEnabled?.Invoke(this, false);
        }
    }
}