public interface IMediaControls
{    
    public void Play();
    public void Pause();
    public void Stop();
    public void SkipToPrevious();
    public void SkipToNext();
    public void FastForward();
    public void Rewind();
    public void SeekTo(long msec);
    public void CycleRepeatOption();
    public void RetrieveMediaInformation();
    public void OpenRelatedActivity();
}