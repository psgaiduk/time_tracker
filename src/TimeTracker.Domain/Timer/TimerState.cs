namespace TimeTracker.Domain.Timer;

public sealed record TimerState(
    TimerPhase Phase,
    TimeSpan Remaining,
    int CompletedWorkIntervals,
    DateTimeOffset? Deadline);
