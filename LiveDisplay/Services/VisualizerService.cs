using Android.Media.Audiofx;
using Android.Util;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media;
using System;

namespace LiveDisplay.Services
{
    public class VisualizerService: Java.Lang.Object, Visualizer.IOnDataCaptureListener
    {
        private Visualizer visualizer;
        private static VisualizerService instance;
        public event EventHandler<byte[]> OnWaveformChanged;
        private VisualizerService()
        {
            Console.WriteLine("Visualizer Service Initialization!");
        }
        public static VisualizerService GetInstance()
        {
            instance ??= new VisualizerService();
            return instance;
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