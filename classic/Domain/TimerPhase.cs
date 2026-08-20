namespace TimeTracker.Classic.Domain
{
    internal enum TimerPhase
    {
        Idle,
        Work,
        Meeting,
        AwaitingBreakDecision,
        WorkSummary,
        ShortBreak,
        LongBreak
    }
}
