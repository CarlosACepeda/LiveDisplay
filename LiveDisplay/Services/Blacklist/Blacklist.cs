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
        public static List<PackageInfo> GetListOfApps(LevelsOfAppBlocking level)
        {
            PackageManager packageManager = Application.Context.PackageManager;
            List<PackageInfo> list = new List<PackageInfo>();
            list = packageManager.GetInstalledPackages(0).ToList();

            switch (level)
            {
                case LevelsOfAppBlocking.None:
                    return list;
                    //No action is done.
                case LevelsOfAppBlocking.Ignore:
                    return list.Where(p => ReturnBlockLevel(p.PackageName) == LevelsOfAppBlocking.Ignore).ToList();
                case LevelsOfAppBlocking.Remove:
                    return list.Where(p => ReturnBlockLevel(p.PackageName) == LevelsOfAppBlocking.Remove).ToList();
                default:
                    return null;
            }
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