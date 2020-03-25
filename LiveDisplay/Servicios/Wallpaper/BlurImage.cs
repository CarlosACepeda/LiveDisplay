using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using System.Threading;

namespace LiveDisplay.Servicios.Wallpaper
{
    public class BlurImage
    {
        private const float Max_Radius = 25;
        private bool async;
        private Context context;
        private Bitmap image;
        private float intensity;

        public BlurImage(Context context)
        {
            this.context = context;
        }

        public BlurImage Intensity(float intensity)
        {
            if (intensity < Max_Radius && intensity > 0)
                this.intensity = intensity;
            else
                this.intensity = Max_Radius;
            return this;
        }

        internal BlurImage Load(Bitmap bitmap)
        {
            image = bitmap;
            return this;
        }

        internal Bitmap GetImageBlur()
        {
            Bitmap imageblurred = null;
            if (async)
            {
                ThreadPool.QueueUserWorkItem(method =>
                {
                    imageblurred = Blur();
                }
                );
            }
            else
            {
                imageblurred = Blur();
            }
            return imageblurred;
        }

        private Bitmap Blur()
        {
            if (image == null)
            {
                return image;
            }

            Bitmap input = Bitmap.CreateScaledBitmap(image, image.Width, image.Height, false); //???

            Bitmap output = Bitmap.CreateBitmap(input);

            RenderScript rs = RenderScript.Create(context);
            ScriptIntrinsicBlur intrinsicBlur = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));

            Allocation inputallocation = Allocation.CreateFromBitmap(rs, input);
            Allocation outputallocation = Allocation.CreateFromBitmap(rs, output);
            intrinsicBlur.SetRadius(intensity);
            intrinsicBlur.SetInput(inputallocation);
            intrinsicBlur.ForEach(outputallocation);

            outputallocation.CopyTo(output);

            return output;
        }

        public BlurImage Async(bool async)
        {
            this.async = async;
            return this;
        }
    }
}