using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Preferences;
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

        public static void ToggleAppBlacklistState(string whichApp, bool isBlacklisted)
        {
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences);
            configurationManager.SaveAValue(whichApp, isBlacklisted);
        }

        public static LevelsOfAppBlocking ReturnBlockLevel(string whichApp)
        {
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences);
            return (LevelsOfAppBlocking)configurationManager.RetrieveAValue(whichApp, 0);
        }
    }

    [Flags]
    public enum LevelsOfAppBlocking
    {
        None = 0,
        Blacklisted = 1,
        NonAllowedToTurnScreenOn = 2,
        BlockInAppOnly = 4,
        TotallyBlocked = Blacklisted | NonAllowedToTurnScreenOn | BlockInAppOnly
    }
}