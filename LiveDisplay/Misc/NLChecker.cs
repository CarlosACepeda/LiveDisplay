using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Provider;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios;

namespace LiveDisplay.Misc
{
    class NLChecker
    {
        public bool IsNotificationListenerEnabled()
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