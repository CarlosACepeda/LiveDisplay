using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System;

namespace LiveDisplay.Misc
{
    internal class PackageUtils
    {
        private static readonly PackageManager packageManager = Application.Context.PackageManager;

        public static string GetTheAppName(string package)
        {
            if (package == null) return string.Empty;
            ApplicationInfo applicationInfo;

            if (Build.VERSION.SdkInt>= BuildVersionCodes.Tiramisu) 
            {
                applicationInfo = packageManager.GetApplicationInfo(package, (PackageInfoFlags)0); //Zero means: No specific PackageInfoFlags specified.

            }

            else 
            {
                applicationInfo = packageManager.GetApplicationInfo(package, 0); //Zero means: No specific PackageInfoFlags specified.
            }
            package = packageManager.GetApplicationLabel(applicationInfo);
            return package;


        }

        public static Intent GetAppIntent(string packageName)
        {
            return packageManager.GetLaunchIntentForPackage(packageName);
        }
    }
}