namespace LiveDisplay.Misc
{
    internal sealed class ConfigurationParameters
    {

        //For LockScreenSettings:
        public const string imagePath = "imagePath";

        public const string hiddenclock = "hiddenclock?";
        public const string hiddensystemicons = "hiddensystemicons?";
        public const string changewallpaper = "changewallpaper";
        public const string blurlevel = "blurlevel";
        public const string opacitylevel = "opacitylevel";
        public const string lockonboot = "lockonboot?";

        //For Saving the device admin state.

        //For Music Widget settings
        public const string showalbumart = "showalbumart?";

        //For Notifications Settings :
        //<The ListenerIsEnabled is not present because that value is get from the Actual Notification Listener (-:>
        public const string enablequickreply = "enablequickreply?";

        //For Awake Settings: (Awake is fused with notification settings now, but here is separated to maintain
        //clearance about which settings are from
        public const string turnonnewnotification = "turnonnewnotification?";
        public const string turnonusermovement = "turnonusermovement?";
        public const string startlockscreendelaytime = "startlockscreendelaytime";
        public const string turnoffscreendelaytime = "turnoffscreendelaytime";
        public const string doubletaptosleep = "doubletaptosleep?";
    }
}