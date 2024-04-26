public interface IMusicControls
{

    public void Play();
    public void Pause();
    public void Stop();
    public void SkipToPrevious();
    public void SkipToNext();
    public void FastForward();
    public void Rewind();
    public void SeekTo(long msec);
    public void RetrieveMediaInformation();
}