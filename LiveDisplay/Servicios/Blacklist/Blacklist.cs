using Android.App;
using Android.Content.PM;
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
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
            configurationManager.SaveAValue(whichApp, isBlacklisted);
        }

        public static LevelsOfAppBlocking ReturnBlockLevel(string whichApp)
        {
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
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
        TotallyBlocked = Blacklisted | NonAllowedToTurnScreenOn | BlockInAppOnly //Invalid, how is totally blocked also 'BlockInAppOnly'?
    }
}