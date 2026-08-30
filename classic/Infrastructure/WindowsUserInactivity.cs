using System;
using System.Runtime.InteropServices;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class WindowsUserInactivity : IUserInactivity
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct LastInputInfo
        {
            internal uint Size;
            internal uint TickCount;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LastInputInfo info);

        public TimeSpan GetInactiveDuration()
        {
            LastInputInfo info = new LastInputInfo();
            info.Size = (uint)Marshal.SizeOf(typeof(LastInputInfo));
            if (!GetLastInputInfo(ref info)) return TimeSpan.Zero;
            uint elapsed = unchecked((uint)Environment.TickCount) - info.TickCount;
            return TimeSpan.FromMilliseconds(elapsed);
        }
    }
}
