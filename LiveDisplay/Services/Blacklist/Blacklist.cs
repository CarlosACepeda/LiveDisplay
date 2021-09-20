using Android.App;
using Android.Content.PM;
using LiveDisplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios
{
    internal class Blacklist
    {
        public static List<PackageInfo> GetListOfApps()
        {
            PackageManager packageManager = Application.Context.PackageManager;

            return packageManager.GetInstalledPackages(0).ToList();
        }

        public static LevelsOfAppBlocking ReturnBlockLevel(string whichApp)
        {
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
            return (LevelsOfAppBlocking)configurationManager.RetrieveAValue(whichApp, 0);
        }

        public static bool AppHasCustomImportanceRules(string whichApp)
        {
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Blacklist);

            if (configurationManager.RetrieveAValue(ConfigurationParameters.AppHasCustomImportanceRules, whichApp) != null)
                return true;

            return false;
        }

        //public NotificationRelevance GetNotificationRelevance(string whichApp)
        //{
        //    ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Blacklist);
        //    return configurationManager.RetrieveAValue(ConfigurationParameters., whichApp);
        //}
        //public NotificationRelevance GetDefaultRelevance(NotificationPriority priority)
        //{
        //}
        //public NotificationRelevance GetDefaultRelevance(NotificationImportance importance)
        //{
        //}
    }

    [Flags]
    public enum LevelsOfAppBlocking
    {
        Default = 0,
        Blacklisted = 1,
        NonAllowedToTurnScreenOn = 2,
        BlockInAppOnly = 4,
        TotallyBlocked = 8,
    }

    //Meant to control how the notification shows in the LockScreen.
    public enum NotificationRelevance
    {
        Ignore, //Don't show it in LiveDisplay.
        Remove, //Remove the notification from Android NotificationDrawer. (when possible)
        AddToList, //Adds the notification to the LiveDisplay notification list.
        Show, //Show this notification in the NotificationWidget.
        Awake, //When Posted, this notification will cause the screen to turn on.
        InterruptMusicWidget, //when Music Widget is active it causes it to hide in order to show this notification
        InterruptOtherNotification, //If another notification is in the Notification Widget, replace it with this one.
        MaximumRelevance = AddToList + Show + Awake + InterruptMusicWidget + InterruptOtherNotification, //Easy template to set High prioirity notifications.
        MinimumRelevance = Ignore + Remove, //Notifications user won't care about.
        DefaultRelevance = AddToList + Show
    }
}