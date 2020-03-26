namespace LiveDisplay.Misc
{
    internal sealed class ConfigurationParameters
    {
        //For LockScreenSettings:
        public const string ImagePath = "imagePath";

        public const string ChangeWallpaper = "changewallpaper";
        public const string BlurLevel = "blurlevel";
        public const string OpacityLevel = "opacitylevel";
        public const string AlbumArtBlurLevel = "albumartblurlevel";
        public const string AlbumArtOpacityLevel = "albumartopacitylevel";
        public const string DefaultWallpaperSettingsAppliesToAlbumArt = "copywallpsettingsfromdefaultwallpaper";
        public const string LockOnBoot = "lockonboot?";
        public const string DoubleTapOnTopActionBehavior = "doubletapontoppactionbehavior";
        public const string DisableWallpaperChangeAnim = "disablewallpaperchangeanim?";

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
        public const string WeatherUpdateRequiresUnlimitedNetwork = "weatherupdaterequiresunlimitednetwork?";
        public const string WeatherUpdateFrequency = "weatherupdatefrequency";
        public const string WeatherUpdateChangesWallpaper = "weatherupdatechangeswallpaper?";

        //For Notifications Settings
        public const string EnableQuickReply = "enablequickreply?";

        //For Awake Settings:
        //clearance about which settings are from
        public const string EnableAwakeService = "enableawake?";

        public const string TurnOnNewNotification = "turnonnewnotification?";
        public const string StartSleepTime = "startsleeptime";
        public const string FinishSleepTime = "finishsleeptime";
        public const string TurnOnUserMovement = "turnonusermovement?";
        public const string StartLockscreenDelayTime = "startlockscreendelaytime";
        public const string TurnOffScreenDelayTime = "turnoffscreendelaytime";
        public const string TurnOffScreenAfterLastNotificationCleared = "turnoffscreenafterlastnotificationcleared?";

        //Misc.
        //It serves the purpose of enabling certain messages to be shown in a Toast Message useful for the developer.
        public const string TestEnabled = "testenabled?";

        //toggles where to show a minitutorial to the user when it launches the lockscreen for the first time.
        public const string TutorialRead = "tutorialread?";
    }
}