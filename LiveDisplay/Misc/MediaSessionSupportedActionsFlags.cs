using System;

[Flags]
public enum MediaSessionSupportedActionsFlags
{
    None = 0,
    Stop= 1,
    Pause = 2,
    Play =4,
    Rewind= 8,
    SkipToPrevious = 16,
    SkipToNext = 32,
    FastForward= 64,
    SetRating= 128,
    SeekTo= 256,
    PlayPause= 512,
    PlayFromMediaId=1024,
    PlayFromSearch=2048,
    SkipToQueueItem= 4096,
    PlayFromUri=8192,
    Prepare= 16384,
    PrepareFromMediaId= 32768,
    PrepareFromSearch= 65536,
    PrepareFromUri= 131072,
    SetPlaybackSpeed= 4194304
}