namespace LiveDisplay.Misc
{
    internal sealed class ConfigurationParameters
    {
        //For LockScreenSettings:
        public const string ImagePath = "imagePath";

        public const string HiddenClock = "hiddenclock?";
        public const string HiddenSystemIcons = "hiddensystemicons?";
        public const string ChangeWallpaper = "changewallpaper";
        public const string BlurLevel = "blurlevel";
        public const string OpacityLevel = "opacitylevel";
        public const string LockOnBoot = "lockonboot?";
        public const string DoubleTapOnTopActionBehavior = "doubletapontoppactionbehavior";

        //For Music Widget settings
        public const string MusicWidgetEnabled = "musicwidgetenabled?";

        public const string ShowAlbumArt = "showalbumart?";

        //For Weather Widget settings
        public const string WeatherCurrent = "weathercurrent";
        public const string WeatherMinimum = "weatherminimum";
        public const string WeatherMaximum = "weathermaximum";
        public const string WeatherLastUpdated = "weatherlastupdated";
        public const string WeatherHumidity = "weatherhumidity";
        public const string WeatherDescription = "weatherdescription";
        public const string WeatherCity = "weathercity";
        public const string WeatherUseImperialSystem = "useimperialsystem?";
        public const string WeatherTemperatureUnit = "weathertemperatureunit";
        public const string WeatherCountryCode = "weathercountrycode";
        public const string WeatherTemperatureMeasureUnit = "weathertemperaturemeasureunit";

        //For Notifications Settings :
        public const string EnableQuickReply = "enablequickreply?";

        //For Awake Settings: (Awake is fused with notification settings now, but here is separated to maintain
        //clearance about which settings are from
        public const string EnableAwakeService = "enableawake?";

        public const string TurnOnNewNotification = "turnonnewnotification?";
        public const string StartSleepTime = "startsleeptime";
        public const string FinishSleepTime = "finishsleeptime";
        public const string TurnOnUserMovement = "turnonusermovement?";
        public const string StartLockscreenDelayTime = "startlockscreendelaytime";
        public const string TurnOffScreenDelayTime = "turnoffscreendelaytime";
        public const string DoubleTapToSleep = "doubletaptosleep?";
    }
}