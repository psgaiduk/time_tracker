using System;
using System.Media;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class WindowsNotificationSound
    {
        internal void PlayBreakCompleted()
        {
            try { SystemSounds.Exclamation.Play(); }
            catch (Exception) { }
        }
    }
}
