using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Misc
{
    sealed class ConfigurationParameters
    {
        public static string enabledLockscreen = "enabledlockscreen?";
        public static string hiddenclock = "hiddenclock?";
        public static string hiddensystemicons = "hiddensystemicons?";
        public static string enabledlockscreennonotifications = "enabledlockscreennonotifications?";
        public static string dynamicwallpaperdisabled = "dynamicwallpaperdisabled?";
    }
}