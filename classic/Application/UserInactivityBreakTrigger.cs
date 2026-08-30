using System;
using TimeTracker.Classic.Domain;

namespace TimeTracker.Classic.Application
{
    internal sealed class UserInactivityBreakTrigger
    {
        private readonly IClock _clock;
        private readonly TimeSpan _threshold;
        private DateTime? _workObservedAt;

        internal UserInactivityBreakTrigger(IClock clock, TimeSpan threshold)
        {
            _clock = clock;
            _threshold = threshold;
        }

        internal static UserInactivityBreakTrigger CreateDefault(IClock clock)
        {
            return new UserInactivityBreakTrigger(clock, TimeSpan.FromMinutes(5));
        }

        internal static UserInactivityBreakTrigger CreateTest(IClock clock)
        {
            return new UserInactivityBreakTrigger(clock, TimeSpan.FromSeconds(5));
        }

        internal bool ShouldStartBreak(TimerPhase phase, TimeSpan inactiveFor)
        {
            DateTime now = _clock.Now;
            if (phase != TimerPhase.Work)
            {
                _workObservedAt = null;
                return false;
            }
            if (!_workObservedAt.HasValue)
            {
                _workObservedAt = now;
                return false;
            }
            if (inactiveFor < _threshold || now - _workObservedAt.Value < _threshold) return false;
            _workObservedAt = now;
            return true;
        }
    }
}
