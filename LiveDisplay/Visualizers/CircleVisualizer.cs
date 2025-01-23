using Android.Views;
using Android.Content;
using Android.Graphics;
using System;
using System.Linq;
using LiveDisplay.Services;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace LiveDisplay.Visualizers
{
    public class CircleVisualizerView : BaseVisualizerView
    {
        public const string Name = "Circle";

        double barHeight;
        double subtendedAngleRad = 0;
        double circumferenceSize = 0;
        double[] barStartCoordinate = new double[2];
        double[] barEndCoordinate = new double[2];
        readonly Circle innerCircle = new Circle();
        double maxBarHeightCurrentBarHeightRatio=0;

        public CircleVisualizerView(Context context, Color visualizerColor) : base(context, visualizerColor)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            //each item in waveform ranges from -127 to 128, representing the lowest point of the wave and the highest point of the wave.
            innerCircle.XPos = canvas.Width / 2; //Center.
            innerCircle.YPos = canvas.Height / 2; //Center.
            innerCircle.Radius = canvas.Width / 3;


            if (WaveForm != null)
            {
                circumferenceSize = (2 * Math.PI) * innerCircle.Radius;
                subtendedAngleRad = (2 * Math.PI) / BarCount;

                for (int bar = 0; bar < BarCount; bar++)
                {
                    double subtendedAngleForBar = bar * subtendedAngleRad;
                    barHeight = WaveForm[bar] - WaveFormSilenceValue; //Offsetting, so the minimum value is 0.
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
                     (float)barEndCoordinate[1], Paint);
                }
            }
            base.OnDraw(canvas);
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