using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Misc
{
    class PackageUtils
    {
        public static string GetTheAppName(string package)
        {
            using (PackageManager packageManager = Application.Context.PackageManager)
            {
                ApplicationInfo applicationInfo = packageManager.GetApplicationInfo(package, 0); //What is zero for?
                package= packageManager.GetApplicationLabel(applicationInfo);
            }
            return package;
        }
    }
}