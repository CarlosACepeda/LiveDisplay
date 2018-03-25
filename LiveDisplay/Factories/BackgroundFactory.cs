using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Renderscripts;

namespace LiveDisplay.Factories
{
    //Areglame: Ejecutame en un  worker thread
    internal class BackgroundFactory
    {
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
            Bitmap blurredBitMapResized= Bitmap.CreateScaledBitmap(blurredBitmap, 70, 80, false);

            Drawable papelTapizDifuminado = new BitmapDrawable(blurredBitMapResized);
            originalBitmap = null;
            blurredBitmap = null;
            blurredBitMapResized = null;
            return papelTapizDifuminado;
        }
    }
}