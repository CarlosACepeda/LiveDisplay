using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Preferences;
using Android.Renderscripts;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;

namespace LiveDisplay.Factories
{
    //TODO: Correct me, I can be optimized.
    internal class BackgroundFactory:Java.Lang.Object
    {
        private static readonly short maxRadius = 25;

        //Retrieves a blurred image
        public static Drawable Difuminar(Drawable papelTapiz, short blurRadius)
        {
            Bitmap originalBitmap = ((BitmapDrawable)papelTapiz).Bitmap;
            Bitmap blurredBitmap= Bitmap.CreateScaledBitmap(originalBitmap,originalBitmap.Width, originalBitmap.Height, false);
            RenderScript rs = RenderScript.Create(Application.Context);
            Allocation input = Allocation.CreateFromBitmap(rs, originalBitmap, Allocation.MipmapControl.MipmapFull, AllocationUsage.Script);
            Allocation output = Allocation.CreateTyped(rs, input.Type);
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));
            script.SetInput(input);
            if(blurRadius< maxRadius)
            {
                script.SetRadius(blurRadius);

            }
            script.ForEach(output);
            output.CopyTo(blurredBitmap);
            Drawable papelTapizDifuminado = new BitmapDrawable(Android.Content.Res.Resources.System,blurredBitmap);
            originalBitmap.Recycle();
            originalBitmap.Dispose();
            blurredBitmap.Recycle();
            blurredBitmap.Dispose();
            input.Dispose();
            output.Dispose();
            return papelTapizDifuminado;
        }
        public string SaveImagePath(Android.Net.Uri uri)
        {
            ContextWrapper contextWrapper = new ContextWrapper(Application.Context);
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(contextWrapper);
            ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences);
            string doc_id = "";
            using (var c1 = Application.Context.ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                string document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = Application.Context.ContentResolver.Query(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
                configurationManager.SaveAValue(ConfigurationParameters.imagePath, path);
            }
            return path;
        }
    }
}