using Android.App;
using Android.Content.PM;

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
    }
}