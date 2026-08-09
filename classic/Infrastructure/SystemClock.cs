using System;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class SystemClock : IClock
    {
        public DateTime Now { get { return DateTime.Now; } }
    }
}
