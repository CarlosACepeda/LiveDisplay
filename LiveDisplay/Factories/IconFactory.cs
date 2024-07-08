namespace LiveDisplay.Factories
{
    using Android.App;
    using Android.Content;
    using Android.Graphics;
    using Android.Graphics.Drawables;

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