using Android.App;
using Android.Media.Audiofx;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media;
using LiveDisplay.Visualizers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Services
{
    public class VisualizerService: Java.Lang.Object, Visualizer.IOnDataCaptureListener
    {
        public string CurrentVisualizerStyle { get; set; } = string.Empty;

        private Visualizer visualizer;
        private static VisualizerService instance;
        public event EventHandler<byte[]> OnWaveformChanged;
        public event EventHandler OnVisualizerStyleChanged;

        private readonly List<string> visualizers = new List<string> 
        { BarVisualizerView.Name, CircleVisualizerView.Name };
        private ConfigurationManager _configurationManager;

        private VisualizerService()
        {
            _configurationManager = new ConfigurationManager();
            CurrentVisualizerStyle = _configurationManager.RetrieveAValue(ConfigurationParameters.CurrentVisualizerStyle, string.Empty);
            Console.WriteLine("Visualizer Service Initialization!");
        }
        public static VisualizerService GetInstance()
        {
            instance ??= new VisualizerService();
            return instance;
        }
        public void CycleVisualizerStyle()
        {
            if (CurrentVisualizerStyle == string.Empty)
            {
                CurrentVisualizerStyle = visualizers.First();
            }
            else
            {
                int currentIndex = visualizers.IndexOf(CurrentVisualizerStyle);
                if (currentIndex == visualizers.Count - 1) currentIndex = 0; //Let's go back to the start.
                else currentIndex++; //if not let's move to the next

                CurrentVisualizerStyle = visualizers.ElementAt(currentIndex);
                _configurationManager.SaveAValue
                (ConfigurationParameters.CurrentVisualizerStyle,
                CurrentVisualizerStyle);
            }
            
        }

        public bool IsActive { get; set; }
        public virtual void OnFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate){
        }

        public virtual void OnWaveFormDataCapture(Visualizer visualizer, byte[] waveform, int samplingRate) 
        {
            this.OnWaveformChanged?.Invoke(null, waveform);
        }

        public void Start()
        {
            if(Checkers.ThisAppCanRecordAudio())
            {
                if (!IsActive)
                {
                    visualizer = new Visualizer(0); //How can I capture audio session from MediaEventsPublisher?
                    int captureSize = Visualizer.GetCaptureSizeRange()[1]; // Get the maximum capture size
                    visualizer.SetCaptureSize(captureSize/4);
                    visualizer.SetDataCaptureListener(this, Visualizer.MaxCaptureRate, true, false);
                    ToggleListeningDataCapture(true);
                    IsActive = true;
                }
                MediaEventsPublisherLollipop.MediaPlaybackChanged += MediaEventsPublisherLollipop_MediaPlaybackChanged;
            }
        }

        private void MediaEventsPublisherLollipop_MediaPlaybackChanged(object sender, Services.Media.MediaEventArgs.MediaPlaybackStateChangedEventArgs e)
        {
            bool isMediaPlaying = e.PlaybackState == Android.Media.Session.PlaybackStateCode.Playing;
            ToggleListeningDataCapture(isMediaPlaying);
        }

        public void Stop()
        {
            ToggleListeningDataCapture(true);
            MediaEventsPublisherLollipop.MediaPlaybackChanged -= MediaEventsPublisherLollipop_MediaPlaybackChanged;
            visualizer?.Release();
            IsActive = false;
        }
        void ToggleListeningDataCapture(bool listening)
        {
            visualizer?.SetEnabled(listening);
        }

    }
}