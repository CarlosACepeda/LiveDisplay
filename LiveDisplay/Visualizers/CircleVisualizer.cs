using Android.Views;
using Android.Content;
using Android.Graphics;
using System;
using System.Linq;
using LiveDisplay.Services;

namespace LiveDisplay.Visualizers
{
    public class CircleVisualizer: Java.Lang.Object
    {
        readonly CircleVisualizerView view;
        Color visualizerColor;
        public CircleVisualizer(CircleVisualizerView view, Color visualizerColor)
        {
            this.view = view;
            this.visualizerColor= visualizerColor;
            VisualizerService.GetInstance().OnWaveformChanged += CircleVisualizer_OnWaveformChanged;
        }

        private void CircleVisualizer_OnWaveformChanged(object sender, byte[] waveform)
        {
            view.Update(waveform, visualizerColor);
        }

        protected override void Dispose(bool disposing)
        {
            VisualizerService.GetInstance().OnWaveformChanged -= CircleVisualizer_OnWaveformChanged; 

            base.Dispose(disposing);
        }
    }
    public class CircleVisualizerView : View
    {
        byte[] waveform;
        readonly Paint paint = new Paint();
        double barWidth;
        double barHeight;
        int barCount = 0;
        double subtendedAngleRad = 0;
        double circumferenceSize = 0;
        double[] barStartCoordinate = new double[2];
        double[] barEndCoordinate = new double[2];
        readonly Circle innerCircle = new Circle();
        double maxBarHeightCurrentBarHeightRatio=0;

        public CircleVisualizerView(Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {

        }
        protected override void OnDraw(Canvas canvas)
        {
            //each item in waveform ranges from -127 to 128, representing the lowest point of the wave and the highest point of the wave.
            innerCircle.XPos = canvas.Width / 2; //Center.
            innerCircle.YPos = canvas.Height / 2; //Center.
            innerCircle.Radius = canvas.Width / 3;


            if (waveform != null)
            {
                barCount = waveform.Count();


                circumferenceSize = (2 * Math.PI) * innerCircle.Radius;
                barWidth = circumferenceSize / barCount;
                subtendedAngleRad = (2 * Math.PI) / barCount;

                for (int bar = 0; bar < barCount; bar++)
                {
                    double subtendedAngleForBar = bar * subtendedAngleRad;
                    barHeight = waveform[bar] - 128; //Offsetting, so the minimum value is 0.
                    maxBarHeightCurrentBarHeightRatio = barHeight / 128;
                    barHeight = barHeight * maxBarHeightCurrentBarHeightRatio;

                    double xStart = (innerCircle.Radius * Math.Cos(subtendedAngleForBar)) + innerCircle.XPos;
                    double yStart = ((innerCircle.Radius * Math.Sin(subtendedAngleForBar) * -1)) + innerCircle.YPos;
                    barStartCoordinate[0] = xStart;
                    barStartCoordinate[1] = yStart;

                    double xEnd = (innerCircle.Radius + barHeight) * Math.Cos(subtendedAngleForBar) + innerCircle.XPos;
                    double yEnd = ((innerCircle.Radius + barHeight) * Math.Sin(subtendedAngleForBar) * -1) + innerCircle.YPos;

                    barEndCoordinate[0] = xEnd;
                    barEndCoordinate[1] = yEnd;


                    canvas.DrawLine(
                     (float)barStartCoordinate[0],
                     (float)barStartCoordinate[1],
                     (float)barEndCoordinate[0],
                     (float)barEndCoordinate[1], paint);
                }

            }
            base.OnDraw(canvas);
        }
        public void Update(byte[] waveform, Color visualizerColor)
        {
            this.waveform = waveform;
            this.paint.SetARGB(visualizerColor.A, visualizerColor.R, visualizerColor.G, visualizerColor.B);
            Invalidate();
        }
    }
    class Circle
    {
        public int Radius { get; set; }
        //x,y in the context of canvas means top left
        //so bottom right should be the highest x,y value.
        public int XPos { get; set; } = 0;
        public int YPos { get; set; } = 0;
    }
}