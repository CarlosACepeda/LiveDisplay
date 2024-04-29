using LiveDisplay.Servicios.Music.MediaEventArgs;
using System;

public interface IMediaEventsPublisher
{
    public void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e);

    public void OnMediaMetadataChanged(EventArgs e);
}