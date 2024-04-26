using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
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
            DevicePolicyManager devicePolicyManager;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                devicePolicyManager = Application.Context.GetSystemService(Java.Lang.Class.FromType(typeof(DevicePolicyManager))) as DevicePolicyManager;
            }
            else
            {
                devicePolicyManager= Application.Context.GetSystemService(Context.DevicePolicyService) as DevicePolicyManager;
            }

            ComponentName componentName = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));

            return devicePolicyManager.IsAdminActive(componentName);
        }

        public static bool ThisAppCanDrawOverlays()
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
                return Settings.CanDrawOverlays(Application.Context);

            return true;
        }

        public static bool ThisAppHasReadStoragePermission()
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
                if (Application.Context.CheckSelfPermission("android.permission.READ_EXTERNAL_STORAGE") == Android.Content.PM.Permission.Granted)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            return true;
        }
        public static bool ThisAppCanPostNotifications()
        {
            NotificationManager notificationManager;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                notificationManager = Application.Context.GetSystemService(Java.Lang.Class.FromType(typeof(NotificationManager))) as NotificationManager;
            }
            else
            {
                notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            }
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                return notificationManager.AreNotificationsEnabled();
            }
            return true;
        }

        public static bool AreMandatoryPermissionsEnabled()
        {
            return IsNotificationListenerEnabled() && ThisAppCanPostNotifications();
        }
    }
}