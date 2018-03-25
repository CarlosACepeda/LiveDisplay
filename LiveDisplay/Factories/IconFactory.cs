using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Factories
{
    class IconFactory
    {
        public static Drawable ReturnIconDrawable(int iconInt, string paquete)
        {
            Context remotePackageContext = null;
            remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            Drawable icon = ContextCompat.GetDrawable(remotePackageContext, iconInt);
            Bitmap iconBitmap = ((BitmapDrawable)icon).Bitmap;
            Bitmap iconBitmapScaled = Bitmap.CreateScaledBitmap(iconBitmap, 36, 36, false);
            Drawable iconDrawableScaled = new BitmapDrawable(iconBitmapScaled);
            icon = null;
            iconBitmap = null;
            iconBitmapScaled = null;
            return iconDrawableScaled;
        }
    }
}