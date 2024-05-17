using System;

public class MediaProgressChangedEventArgs : EventArgs
{
    public long CurrentProgress { get; set; }
    public long TotalProgress { get; set; } = 0;
}