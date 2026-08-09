using System;

namespace TimeTracker.Classic.Application
{
    internal interface IClock
    {
        DateTime Now { get; }
    }
}
