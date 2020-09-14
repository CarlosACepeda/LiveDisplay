using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Views;
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
        private int deviceHeight;
        private int deviceWidth;

        public BlurImage(Context context)
        {
            this.context = context;
            deviceHeight = this.context.Resources.DisplayMetrics.HeightPixels;
            deviceWidth = this.context.Resources.DisplayMetrics.WidthPixels;
        }

        public BlurImage Intensity(float intensity)
        {
            if (intensity < Max_Radius && intensity > 0)
                this.intensity = intensity;
            else if (intensity == 0 || intensity < 0)
                this.intensity = 0;
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
            if (intensity == 0)
            {
                return image; //No need to blur the image.
            }

            Bitmap input;
            if (image.Width > deviceWidth || image.Height > deviceHeight)
            {
                input = Bitmap.CreateScaledBitmap(image, deviceWidth, deviceHeight, false);
            }
            else { input = image; }

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