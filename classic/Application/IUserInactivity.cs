using System;

namespace TimeTracker.Classic.Application
{
    internal interface IUserInactivity
    {
        TimeSpan GetInactiveDuration();
    }
}
