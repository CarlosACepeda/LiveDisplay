using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.Provider;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Servicios;

namespace LiveDisplay.Misc
{
    internal class Checkers
    {
        public static bool IsNotificationListenerEnabled()
        {
            ComponentName cn = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(Catcher)).Name);
            string flat = Settings.Secure.GetString(Application.Context.ContentResolver, "enabled_notification_listeners");
            if (flat != null && flat.Contains(cn.FlattenToString()))
            {
                return true;
            }
            return false;
        }

        public static bool IsThisAppADeviceAdministrator()
        {
            DevicePolicyManager devicePolicyManager = Application.Context.GetSystemService(Context.DevicePolicyService) as DevicePolicyManager;

            ComponentName componentName = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));

            return devicePolicyManager.IsAdminActive(componentName);
        }

        public static bool ThisAppCanDrawOverlays()
        {
            return Settings.CanDrawOverlays(Application.Context);
        }
    }
}