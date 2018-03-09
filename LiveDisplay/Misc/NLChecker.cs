using Android.App;
using Android.Content;
using Android.Provider;
using LiveDisplay.Servicios;
using System;

namespace LiveDisplay.Misc
{
    internal class NLChecker
    {
        public static bool IsNotificationListenerEnabled()
        {
            ComponentName cn = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(Catcher)).Name);
            String flat = Settings.Secure.GetString(Application.Context.ContentResolver, "enabled_notification_listeners");
            if (flat != null && flat.Contains(cn.FlattenToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}