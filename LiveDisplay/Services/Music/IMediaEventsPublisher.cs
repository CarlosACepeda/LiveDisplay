using Android.Graphics;
using LiveDisplay.Services.Media.Enums;
using LiveDisplay.Services.Media.MediaEventArgs;
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

    public MediaSessionSupportedActionsFlags GetSupportedActions();

    string GetStringValue<TKey>(TKey metadataKey);
    long GetLongValue<TKey>(TKey metadataKey);
    Bitmap GetBitmap<TKey>(TKey metadataKey);
    AvailableControls SetAvailableControls(MediaSessionSupportedActionsFlags supportedActionsFlags);
}