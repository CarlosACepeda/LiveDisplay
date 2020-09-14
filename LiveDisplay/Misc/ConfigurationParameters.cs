namespace LiveDisplay.Misc
{
    internal sealed class ConfigurationParameters
    {
        public const string ImagePath = "imagePath";
        public const string ChangeWallpaper = "changewallpaper";
        public const string BlurLevel = "blurlevel";
        public const int DefaultBlurLevel = 1;
        public const int DefaultAlbumartBlurLevel = 16;
        public const string OpacityLevel = "opacitylevel";
        public const int DefaultOpacityLevel =100;
        public const int DefaultAlbumartOpacityLevel = 40;
        public const string AlbumArtBlurLevel = "albumartblurlevel";
        public const string AlbumArtOpacityLevel = "albumartopacitylevel";
        public const string DefaultWallpaperSettingsAppliesToAlbumArt = "copywallpsettingsfromdefaultwallpaper";
        public const string LockOnBoot = "lockonboot?";
        public const string DoubleTapOnTopActionBehavior = "doubletapontoppactionbehavior";
        public const string DisableWallpaperChangeAnim = "disablewallpaperchangeanim?";
        public const string MusicWidgetEnabled = "musicwidgetenabled?";
        public const string ShowAlbumArt = "showalbumart?";
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
        public const string EnableQuickReply = "enablequickreply?";
        public const string EnableAwakeService = "enableawake?";
        public const string ListenForDeviceMotion = "listenfordevicemotion?";
        public const string TurnOnNewNotification = "turnonnewnotification?";
        public const string AwakeCausesBlackWallpaper = "awakecausesblackwallpaper?";
        public const string StartSleepTime = "startsleeptime";
        public const string FinishSleepTime = "finishsleeptime";
        public const string TurnOnUserMovement = "turnonusermovement?";
        public const string StartLockscreenDelayTime = "startlockscreendelaytime";
        public const string TurnOffScreenDelayTime = "turnoffscreendelaytime";
        public const string TurnOffScreenAfterLastNotificationCleared = "turnoffscreenafterlastnotificationcleared?";
        public const string MusicWidgetMethod = "musicwidgetcontrolmethod"; //Grab a session from the notification or from the media sessions listener.
        public const string HideNotificationWhenItsMediaPlaying = "hidenotificationwhenmediaplaying?"; //If the media session used belongs to a notification then hide this notification in the lockscreen.
        public const string HideShortcutsWhenKeyguardSafe = "hideshortcutswhenkeyguardsafe?";

        //Misc.
        //It serves the purpose of enabling certain messages to be shown in a Toast Message useful for the developer.
        public const string TestEnabled = "testenabled?";

        //toggles where to show a minitutorial to the user when it launches the lockscreen for the first time.
        public const string TutorialRead = "tutorialread?";

        //Never used by the user: Which widget should be shown first when starting the lockscreen.
        //By default it is the clock. (possible values: "clock", "music", "notification")
        //When there's not an active widget.
        public const string StartingWidget = "clock";


    }
}