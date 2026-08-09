using System;

namespace TimeTracker.Classic.Domain
{
    internal sealed class TimerState
    {
        internal TimerState(TimerPhase phase, TimeSpan remaining, int completedWorkIntervals)
        {
            Phase = phase;
            Remaining = remaining;
            CompletedWorkIntervals = completedWorkIntervals;
        }

        internal TimerPhase Phase { get; private set; }
        internal TimeSpan Remaining { get; private set; }
        internal int CompletedWorkIntervals { get; private set; }
    }
}
