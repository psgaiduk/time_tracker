using System;

namespace TimeTracker.Classic.Domain
{
    internal sealed class TimerState
    {
        internal TimerState(TimerPhase phase, TimeSpan remaining, int completedWorkIntervals)
            : this(phase, remaining, completedWorkIntervals, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero)
        {
        }

        internal TimerState(TimerPhase phase, TimeSpan remaining, int completedWorkIntervals, TimeSpan shortBreakBalance, TimeSpan longBreakBalance, TimeSpan overdue, TimeSpan breakDuration)
        {
            Phase = phase;
            Remaining = remaining;
            CompletedWorkIntervals = completedWorkIntervals;
            ShortBreakBalance = shortBreakBalance;
            LongBreakBalance = longBreakBalance;
            Overdue = overdue;
            BreakDuration = breakDuration;
        }

        internal TimerPhase Phase { get; private set; }
        internal TimeSpan Remaining { get; private set; }
        internal int CompletedWorkIntervals { get; private set; }
        internal TimeSpan ShortBreakBalance { get; private set; }
        internal TimeSpan LongBreakBalance { get; private set; }
        internal TimeSpan Overdue { get; private set; }
        internal TimeSpan BreakDuration { get; private set; }
    }
}
