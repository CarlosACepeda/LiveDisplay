using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Renderscripts;
using Android.Util;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;

namespace LiveDisplay.Factories
{
    internal class BackgroundFactory
    {
        //Retrieves a default lockscreen
        public Drawable Difuminar(Drawable papelTapiz)
        {
            //Fondo de escritorio provista por el Argumento que se pasa en <papelTapiz>
            Bitmap originalBitmap = ((BitmapDrawable)papelTapiz).Bitmap;
            // Un bitmap null que almacenará la imagen difuminada.
            Bitmap blurredBitmap;
            //Asignar a este bitmap la imagen original para trabajar con ella.
            blurredBitmap = Bitmap.CreateBitmap(originalBitmap);
            //Crear la instancia de RenderScript que hará el trabajo
            RenderScript rs = RenderScript.Create(Application.Context);
            //Alocar memoria para que RenderScript trabaje.
            Allocation input = Allocation.CreateFromBitmap(rs, originalBitmap, Allocation.MipmapControl.MipmapFull, AllocationUsage.Script);
            Allocation output = Allocation.CreateTyped(rs, input.Type);

            // Load up an instance of the specific script that we want to use.
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));
            script.SetInput(input);

            // Set the blur radius
            script.SetRadius(25);

            // Start the ScriptIntrinisicBlur
            script.ForEach(output);

            // Copy the output to the blurred bitmap
            output.CopyTo(blurredBitmap);

            //Scale the bitmap:
            Bitmap blurredBitMapResized = Bitmap.CreateScaledBitmap(blurredBitmap, 70, 80, false);

            Drawable papelTapizDifuminado = new BitmapDrawable(blurredBitMapResized);
            originalBitmap = null;
            blurredBitmap = null;
            blurredBitMapResized = null;
            return papelTapizDifuminado;
        }
        public Drawable Difuminar(Bitmap image)
        {            
            Bitmap blurredBitmap;
            //Asignar a este bitmap la imagen original para trabajar con ella.
            blurredBitmap = image;
            //Crear la instancia de RenderScript que hará el trabajo
            RenderScript rs = RenderScript.Create(Application.Context);
            //Alocar memoria para que RenderScript trabaje.
            Allocation input = Allocation.CreateFromBitmap(rs, image, Allocation.MipmapControl.MipmapFull, AllocationUsage.Script);
            Allocation output = Allocation.CreateTyped(rs, input.Type);

            // Load up an instance of the specific script that we want to use.
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));
            script.SetInput(input);

            // Set the blur radius
            script.SetRadius(25);

            // Start the ScriptIntrinisicBlur
            script.ForEach(output);

            // Copy the output to the blurred bitmap
            output.CopyTo(blurredBitmap);

            //Scale the bitmap:
            Bitmap blurredBitMapResized = Bitmap.CreateScaledBitmap(blurredBitmap, 70, 80, false);

            Drawable papelTapizDifuminado = new BitmapDrawable(blurredBitMapResized);
            image = null;
            blurredBitmap = null;
            blurredBitMapResized = null;
            return papelTapizDifuminado;
        }
        public string SaveImagePath(Uri uri)
        {
            ContextWrapper contextWrapper = new ContextWrapper(Application.Context);
            ConfigurationManager configuration = new ConfigurationManager(contextWrapper.GetSharedPreferences("livedisplayconfig", FileCreationMode.Private));
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
                Log.Info("Path is", path);
                configuration.SaveAValue(ConfigurationParameters.imagePath, path);
            }
            return path;
            
        }
    }
}