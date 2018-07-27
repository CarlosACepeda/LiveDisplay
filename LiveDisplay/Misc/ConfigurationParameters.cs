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
        public const string istheappconfigured = "istheappconfigured?";

        //For LockScreenSettings:
        public const string imagePath = "imagePath";
        //Deleted one string, useless.
        public const string hiddenclock = "hiddenclock?";
        public const string hiddensystemicons = "hiddensystemicons?";
        //Functional
        public const string enabledlockscreennonotifications = "enabledlockscreennonotifications?";
        public const string dynamicwallpaperenabled = "dynamicwallpaperenabled?";
        //For Notifications Settings :
        //<The ListenerIsEnabled is not present because that value is get from the Actual Notification Listener (-:>
        public const string showstickynotifications = "showstickynotifications?";
        public const string enablequickreply = "enablequickreply?";
        //For Awake Settings: (Awake is fused with notification settings now, but here is separated to maintain
        //clearance about which settings are from
        public const string turnonnewnotification = "turnonnewnotification?";
        public const string turnonusermovement = "turnonusermovement?";
        public const string startlockscreendelaytime = "startlockscreendelaytime";
        public const string turnoffscreendelaytime = "turnoffscreendelaytime";


    }
}