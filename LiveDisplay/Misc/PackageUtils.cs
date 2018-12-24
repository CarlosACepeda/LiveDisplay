using Android.App;
using Android.Content.PM;

namespace LiveDisplay.Misc
{
    internal class PackageUtils
    {
        public static string GetTheAppName(string package)
        {
            using (PackageManager packageManager = Application.Context.PackageManager)
            {
                ApplicationInfo applicationInfo = packageManager.GetApplicationInfo(package, 0); //Zero means: No specific PackageInfoFlags specified.
                package = packageManager.GetApplicationLabel(applicationInfo);
            }
            return package;
        }
    }
}