using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Preferences;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios
{
    internal class Blacklist
    {
        public static List<PackageInfo> GetListOfApps()
        {
            PackageManager packageManager = Application.Context.PackageManager;

            var lol = packageManager.GetInstalledPackages(0);
            return lol.ToList();
        }

        public static void ToggleAppBlacklistState(string whichApp, bool isBlacklisted)
        {
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences);
            configurationManager.SaveAValue(whichApp, isBlacklisted);
        }

        public static bool IsAppBlacklisted(string whichApp)
        {
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences);
            return configurationManager.RetrieveAValue(whichApp);
        }
    }
}