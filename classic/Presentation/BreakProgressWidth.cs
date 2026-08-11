using System;

namespace TimeTracker.Classic.Presentation
{
    internal static class BreakProgressWidth
    {
        internal static int Calculate(int totalWidth, TimeSpan remaining, TimeSpan duration)
        {
            if (totalWidth <= 0 || remaining <= TimeSpan.Zero || duration <= TimeSpan.Zero) return 0;
            if (remaining >= duration) return totalWidth;
            return (int)Math.Round(totalWidth * remaining.TotalMilliseconds / duration.TotalMilliseconds);
        }
    }
}
