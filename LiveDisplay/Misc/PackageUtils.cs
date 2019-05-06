using Android.App;
using Android.Content.PM;

namespace LiveDisplay.Misc
{
    internal class PackageUtils
    {
        //TODO: Make the Package manager lazy loaded, to avoid creating each time new instance of PackageManager.
        //This method is called multiple times in a matter of seconds, it might be possible that it will throw a
        //StackOverflowException due to multiple instances of PackageManager.
        public static string GetTheAppName(string package)
        {
            PackageManager packageManager = Application.Context.PackageManager;
            ApplicationInfo applicationInfo = packageManager.GetApplicationInfo(package, 0); //Zero means: No specific PackageInfoFlags specified.
            package = packageManager.GetApplicationLabel(applicationInfo);
            return package;
        }
    }
}