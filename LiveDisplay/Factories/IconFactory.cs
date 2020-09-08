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

        public static Drawable ReturnActionIconDrawable(Icon icon, string paquete)
        {
            Context remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            return icon.LoadDrawable(remotePackageContext);
        }

        //Overload for Retrieving Action Buttons in Lollipop and less.
        public static Drawable ReturnActionIconDrawable(int icon, string paquete)
        {
            Context remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
            return remotePackageContext.Resources.GetDrawable(icon);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
        }
    }
}