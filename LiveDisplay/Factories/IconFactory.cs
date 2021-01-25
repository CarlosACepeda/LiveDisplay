namespace LiveDisplay.Factories
{
    using Android.App;
    using Android.Content;
    using Android.Graphics.Drawables;
    using AndroidX.Core.Content;

    internal class IconFactory
    {
        public static Drawable ReturnIconDrawable(int iconInt, string paquete)
        {
            Context remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            Drawable icon = ContextCompat.GetDrawable(remotePackageContext, iconInt);
            return icon;
        }

        public static Drawable ReturnIconDrawable(Icon icon, string paquete)
        {
            Context remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            return icon.LoadDrawable(remotePackageContext);
        }

        public static Drawable ReturnActionIconDrawable(Icon icon, string paquete)
        {
            Context remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            return icon.LoadDrawable(remotePackageContext);
        }

        //Overload for Retrieving Action Buttons in Lollipop and less.
        [System.Obsolete]
        public static Drawable ReturnActionIconDrawable(int icon, string paquete)
        {
            Context remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            return remotePackageContext.Resources.GetDrawable(icon);
        }
    }
}