﻿using System;
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
        //Deleted one string, useless.
        public static string hiddenclock = "hiddenclock?";
        public static string hiddensystemicons = "hiddensystemicons?";
        //Functional
        public static string enabledlockscreennonotifications = "enabledlockscreennonotifications?";
        public static string dynamicwallpaperenabled = "dynamicwallpaperenabled?";
        //For Notifications Settings :
        //<The ListenerIsEnabled is not present because that value is get from the Actual Notification Listener (-:>
        public static string showstickynotifications = "showstickynotifications?";
        public static string enablequickreply = "enablequickreply?";
        //For Awake Settings: (Awake is fused with notification settings now, but here is separated to maintain
        //clearance about which settings are from
        public static string turnonnewnotification = "turnonnewnotification?";
        public static string turnonusermovement = "turnonusermovement?";
        public static string startlockscreendelaytime = "startlockscreendelaytime";
        public static string turnoffscreendelaytime = "turnoffscreendelaytime";


    }
}