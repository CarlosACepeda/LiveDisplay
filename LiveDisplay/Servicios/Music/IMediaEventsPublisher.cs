using LiveDisplay.Servicios.Music.MediaEventArgs;
using System;

public interface IMediaEventsPublisher
{
    public const int DontRepeat = 0;
    public const int RepeatOnce = 1;
    public const int RepeatForever = 2;
    public void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e);

    public void OnMediaMetadataChanged(EventArgs e);

    public void OnMediaProgressChanged(MediaProgressChangedEventArgs e);
    public void OnMediaRepeatOptionChanged(int newOption);
}