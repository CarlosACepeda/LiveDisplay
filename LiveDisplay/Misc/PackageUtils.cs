using Android.App;
using Android.Content.PM;
using Android.OS;
using System;

namespace LiveDisplay.Misc
{
    internal class PackageUtils
    {
        private static PackageManager packageManager = Application.Context.PackageManager;

        public static string GetTheAppName(string package)
        {
            if (package == null) return string.Empty;

            ApplicationInfo applicationInfo = packageManager.GetApplicationInfo(package, 0); //Zero means: No specific PackageInfoFlags specified.
            try
            {
                package = packageManager.GetApplicationLabel(applicationInfo);
            }
            catch
            {
                package = null;
            }
            return package;
        }

        internal static string GetAppVersionName()
        {
            PackageInfo packageInfo = packageManager.GetPackageInfo(Application.Context.PackageName, 0); //Zero means: No specific PackageInfoFlags specified.
            string versionCode;

            if(Build.VERSION.SdkInt< BuildVersionCodes.P)
            {
                versionCode = packageInfo.VersionCode.ToString();
            }
            else
            {
                versionCode = packageInfo.LongVersionCode.ToString();
            }
            try
            {
                return packageInfo.VersionName + " Build Number "+ versionCode + "";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}