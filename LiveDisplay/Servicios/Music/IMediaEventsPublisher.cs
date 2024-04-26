using System;

public interface IMediaEventsPublisher
{
    public void OnMediaPlaybackChanged(EventArgs e);

    public void OnMediaMetadataChanged(EventArgs e);
}