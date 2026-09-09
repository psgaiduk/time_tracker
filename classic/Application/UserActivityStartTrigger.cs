using System;

namespace TimeTracker.Classic.Application
{
    internal sealed class UserActivityStartTrigger
    {
        private bool _observed;
        private TimeSpan _previousInactiveFor;

        internal bool ShouldStart(DateTime now, TimeSpan inactiveFor, TimeSpan workDayStart, TimeSpan workDayEnd)
        {
            bool within = workDayEnd > workDayStart && now.TimeOfDay >= workDayStart && now.TimeOfDay < workDayEnd;
            bool started = _observed && within && inactiveFor < _previousInactiveFor;
            _previousInactiveFor = inactiveFor;
            _observed = true;
            return started;
        }
    }
}
