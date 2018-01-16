using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Factories
{
    class BackgroundFactory
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

            Drawable papelTapizDifuminado = new BitmapDrawable(blurredBitmap);
            return papelTapizDifuminado;

        }

    }
}