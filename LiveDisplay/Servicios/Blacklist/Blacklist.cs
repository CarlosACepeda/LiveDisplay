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

        public static LevelsOfAppBlocking IsAppBlacklisted(string whichApp)
        {
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences);
            var lol= (LevelsOfAppBlocking)configurationManager.RetrieveAValue(whichApp, 0);
            return lol;
        }
    }

    [Flags]
    public enum LevelsOfAppBlocking
    {
        None = 0,
        Blacklisted = 1,
        NonAllowedToTurnScreenOn = 2,
        TotallyBlocked = Blacklisted | NonAllowedToTurnScreenOn
    }
}