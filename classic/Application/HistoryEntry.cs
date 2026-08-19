using System;

namespace TimeTracker.Classic.Application
{
    internal sealed class HistoryEntry
    {
        internal HistoryEntry(ActivityKind kind, DateTime startedAt, DateTime finishedAt, TimeSpan plannedDuration, TimeSpan shortBreakBalance, TimeSpan longBreakBalance)
        {
            Kind = kind;
            StartedAt = startedAt;
            FinishedAt = finishedAt;
            PlannedDuration = plannedDuration;
            ShortBreakBalance = shortBreakBalance;
            LongBreakBalance = longBreakBalance;
        }

        internal ActivityKind Kind { get; private set; }
        internal DateTime StartedAt { get; private set; }
        internal DateTime FinishedAt { get; private set; }
        internal TimeSpan PlannedDuration { get; private set; }
        internal TimeSpan ShortBreakBalance { get; private set; }
        internal TimeSpan LongBreakBalance { get; private set; }
    }
}
