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
        //For Deciding if show the SplashScreen or not.
        public static string istheappconfigured = "istheappconfigured?";

        //For LockScreenSettings:
        public static string imagePath = "imagePath";
        public static string enabledLockscreen = "enabledlockscreen?";
        public static string hiddenclock = "hiddenclock?";
        public static string hiddensystemicons = "hiddensystemicons?";
        public static string enabledlockscreennonotifications = "enabledlockscreennonotifications?";
        public static string dynamicwallpaperenabled = "dynamicwallpaperenabled?";
        //For Notifications Settings :
        //<The ListenerIsEnabled is not present because that value is get from the Actual Notification Listener (-:>
        public static string showstickynotifications = "showstickynotifications?";
        public static string enablequickreply = "enablequickreply?";
        //For Awake Settings:
        public static string turnonnewnotification = "turnonnewnotification?";
        public static string turnonusermovement = "turnonusermovement";

    }
}