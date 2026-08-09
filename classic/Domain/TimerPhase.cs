namespace TimeTracker.Classic.Domain
{
    internal enum TimerPhase
    {
        Idle,
        Work,
        AwaitingBreakDecision,
        ShortBreak,
        LongBreak
    }
}
