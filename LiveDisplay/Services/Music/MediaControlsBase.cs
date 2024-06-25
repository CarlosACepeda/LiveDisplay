using Android.Media.Session;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media.MediaEventArgs;
using System;

public class MediaControlsBase
{
    /// <summary>
    /// Use this class to Send Media events to RemoteControllers and MediaControllers
    /// </summary>
    public event EventHandler<MediaActionEventArgs> MediaEvent;
    private static MediaControlsBase _instance;

    public virtual void OnMediaEvent(MediaActionEventArgs e) {
        Console.WriteLine($"MediaEvent subscriber count {MediaEvent?.GetInvocationList().Length}");

        MediaEvent?.Invoke(this, e);
    }
    private MediaControlsBase()
    {

    }
    public static MediaControlsBase Instance => _instance ??= new MediaControlsBase();


    public void Play()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.Play
        });
    }

    public void Pause()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.Pause
        });
    }

    public void SkipToPrevious()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.SkipToPrevious
        });
    }

    public void SeekTo(long time)
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.SeekTo,
            Time = time
        });
    }

    public void FastForward()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.FastFoward
        });
    }

    public void Rewind()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.Rewind
        });
    }

    public void SkipToNext()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.SkipToNext
        });
    }

    public void Stop()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.Stop
        });
    }

    public void RetrieveMediaInformation()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.RetrieveMediaInformation
        });
    }

    public void CycleRepeatOption()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.CycleRepeatOption,
        });
    }

    public void OpenRelatedActivity()
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.OpenRelatedActivity
        });
    }

    public void SendCustomAction(PlaybackState.CustomAction customAction)
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.SendCustomAction,
            CustomAction = customAction
        });
    }
    public void SendCompactedAction(OpenAction compactedAction)
    {
        OnMediaEvent(new MediaActionEventArgs
        {
            MediaActionFlags = MediaActionFlags.SendCompactedAction,
            CompactedAction = compactedAction
        });
    }
}