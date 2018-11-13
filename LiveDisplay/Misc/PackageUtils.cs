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
                ApplicationInfo applicationInfo = packageManager.GetApplicationInfo(package, 0); //What is zero for?
                package = packageManager.GetApplicationLabel(applicationInfo);
            }
            return package;
        }
    }
}