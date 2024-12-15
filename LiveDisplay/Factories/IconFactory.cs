namespace LiveDisplay.Factories
{
    using Android.App;
    using Android.Content;
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.Util;

    internal class IconFactory
    {
        Context remotePackageContext;
        Drawable currentDrawable;

        public IconFactory(int iconInt, string package) 
        {
            remotePackageContext = Application.Context.CreatePackageContext(package, 0);
            currentDrawable = remotePackageContext.GetDrawable(iconInt);
        }
        public IconFactory(Icon icon, string package)
        {
            remotePackageContext = Application.Context.CreatePackageContext(package, 0);
            currentDrawable = icon.LoadDrawable(remotePackageContext);
        }
        public IconFactory(Drawable icon)
        {
            remotePackageContext = Application.Context; //this time we are working with a drawable we own. not from an external package
            currentDrawable= icon;
        }

        public IconFactory ApplyColorFilter(Color color)
        {
            currentDrawable.SetColorFilter(new BlendModeColorFilter(color, BlendMode.SrcAtop));
            return this;
        }
        public IconFactory ResizeDrawable(int widthInPixels, int heigthInPixels) 
        {
            Bitmap bitmap = DrawableToBitmap(currentDrawable);
            Drawable d = new BitmapDrawable(remotePackageContext.Resources, Bitmap.CreateScaledBitmap(bitmap, widthInPixels, heigthInPixels, true));
            currentDrawable = d;
            return this;
        }
        public IconFactory ResizeDrawableDp(int widthInDp, int heightInDp)
        {
            int heightInPx = (int)TypedValue.ApplyDimension(
             ComplexUnitType.Dip, heightInDp, Application.Context.Resources.DisplayMetrics);

            int widthInPx = (int)TypedValue.ApplyDimension(
            ComplexUnitType.Dip, widthInDp, Application.Context.Resources.DisplayMetrics);
            return ResizeDrawable(widthInPx, heightInPx);

        }
        Bitmap DrawableToBitmap(Drawable drawable)
        {
            Bitmap bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth,
                    drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);

            return bitmap;
        }

        public Drawable Build()
        {
            return currentDrawable;
        }
    }
}