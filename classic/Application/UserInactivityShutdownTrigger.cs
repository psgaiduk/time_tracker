using System;

namespace TimeTracker.Classic.Application
{
    internal sealed class UserInactivityShutdownTrigger
    {
        private readonly TimeSpan _threshold;
        internal UserInactivityShutdownTrigger(TimeSpan threshold) { _threshold = threshold; }
        internal static UserInactivityShutdownTrigger CreateDefault() { return new UserInactivityShutdownTrigger(TimeSpan.FromMinutes(3)); }
        internal static UserInactivityShutdownTrigger CreateTest() { return new UserInactivityShutdownTrigger(TimeSpan.FromSeconds(3)); }
        internal bool ShouldShutdown(DateTime now, TimeSpan inactiveFor, TimeSpan workDayStart, TimeSpan workDayEnd)
        {
            if (workDayEnd <= workDayStart) return false;
            TimeSpan time = now.TimeOfDay;
            if (time >= workDayStart && time < workDayEnd) return false;
            return inactiveFor >= _threshold;
        }
    }
}
