namespace LiveDisplay.Services.Media.Enums
{
    public enum AvailableControls
    {
        None = 0,
        PlayPause=1,
        SkipToNext=2,
        SkipToPrevious=4,
        Repeat= 8,
        Stop= 16,
        SkipToNextCustom= 32,
        SkipToPreviousCustom= 64,
        CustomActionOne= 128,
        CustomActionTwo= 256,
        Buffering= 512
    }
}
