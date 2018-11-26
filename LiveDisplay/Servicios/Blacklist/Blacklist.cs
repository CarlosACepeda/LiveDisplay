using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Blacklist
{
    class Blacklist
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