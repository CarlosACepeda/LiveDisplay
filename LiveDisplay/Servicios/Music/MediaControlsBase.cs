using LiveDisplay.Servicios.Music.MediaEventArgs;
using System;

public class MediaControlsBase
{
    public event EventHandler<MediaActionEventArgs> MediaEvent;

    public virtual void OnMediaEvent(MediaActionEventArgs e) {
        Console.WriteLine($"MediaEvent subscriber count {MediaEvent?.GetInvocationList().Length}");

        MediaEvent?.Invoke(this, e);
    }
}