using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;

namespace LiveDisplay.Factories
{
    internal class IconFactory
    {
        public static Drawable ReturnIconDrawable(int iconInt, string paquete)
        {
            Context remotePackageContext = null;
            remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            Drawable icon = ContextCompat.GetDrawable(remotePackageContext, iconInt);
            remotePackageContext = null;
            return icon;
        }

        public static Drawable ReturnActionIconDrawable(Icon icon, string paquete)
        {
            Context remotePackageContext = null;
            remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            return icon.LoadDrawable(remotePackageContext);
        }

        //Overload for Retrieving Action Buttons on Lollipop an less.
        public static Drawable ReturnActionIconDrawable(int icon, string paquete)
        {
            Context remotePackageContext = null;
            remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            return remotePackageContext.GetDrawable(icon);
        }
    }
}