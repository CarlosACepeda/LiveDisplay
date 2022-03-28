using Android.App;
using Android.Content.PM;
using LiveDisplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Services
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
    }

    [Flags]
    public enum LevelsOfAppBlocking
    {
        None=0,
        Ignore=1,
        Remove=2
        
    }
}