namespace TimeTracker.Domain.Timer;

public enum TimerPhase
{
    Idle,
    Work,
    AwaitingBreakDecision,
    ShortBreak,
    LongBreak
}
