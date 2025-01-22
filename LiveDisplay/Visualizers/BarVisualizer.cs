using Android.Content;
using Android.Graphics;

namespace LiveDisplay.Visualizers
{
    public class BarVisualizerView : BaseVisualizerView
    {
        public const string Name = "Bar";
        public BarVisualizerView(Context context, Color visualizerColor) : base(context, visualizerColor)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (WaveForm != null)
            {
                int distanceBetweenBars = canvas.Width / BarCount;
                int yStart = (canvas.Height/2)- WaveFormSilenceValue;

                for (int bar = 1; bar <= BarCount; bar++)
                {

                    int xStart = bar * distanceBetweenBars;

                    int yEnd = yStart + (WaveForm[bar - 1]);

                    canvas.DrawLine(xStart, yStart+ WaveFormSilenceValue, xStart + 1, yEnd, Paint);
                }
            }

            base.OnDraw(canvas);
        }
    }
}