using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using LiveDisplay.Services;
using System;
using System.Linq;

namespace LiveDisplay.Visualizers
{
    public class BaseVisualizerView : View
    {
        protected byte[] WaveForm { get; set; }
        protected Paint Paint { get; set; } = new Paint();
        protected Color VisualizerColor { get; set; }

        protected int BarCount { get => WaveForm==null?0: WaveForm.Count(); }
        protected int WaveFormSilenceValue { get => 127; }

        public BaseVisualizerView(Context context, Color visualizerColor) : base(context)
        {
            LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
    ViewGroup.LayoutParams.MatchParent);
            VisualizerColor = visualizerColor;
            VisualizerService.GetInstance().OnWaveformChanged += BaseVisualizer_OnWaveformChanged;

        }

        protected void BaseVisualizer_OnWaveformChanged(object sender, byte[] waveform)
        {
            Update(waveform, VisualizerColor);

        }
        public void Update(byte[] waveform, Color visualizerColor)
        {
            WaveForm = waveform;
            Paint.SetARGB(visualizerColor.A, visualizerColor.R, visualizerColor.G, visualizerColor.B);
            Invalidate();
        }
        protected override void Dispose(bool disposing)
        {
            VisualizerService.GetInstance().OnWaveformChanged -= BaseVisualizer_OnWaveformChanged;
            base.Dispose(disposing);
        }
    }
}