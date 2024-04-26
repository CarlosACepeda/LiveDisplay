using Android.App;
using Android.Content.PM;
using Android.OS;

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