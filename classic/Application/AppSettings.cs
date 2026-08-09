namespace TimeTracker.Classic.Application
{
    internal sealed class AppSettings
    {
        internal AppSettings()
        {
            HideOverlayFromCapture = true;
            StartWithWindows = false;
        }

        internal bool HideOverlayFromCapture { get; set; }
        internal bool StartWithWindows { get; set; }
    }
}
