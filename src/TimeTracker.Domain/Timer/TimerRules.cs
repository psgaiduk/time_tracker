namespace TimeTracker.Domain.Timer;

public sealed record TimerRules
{
    public static TimerRules Default { get; } = new();

    public static TimerRules Test { get; } = new()
    {
        WorkDuration = TimeSpan.FromSeconds(25),
        ShortBreakDuration = TimeSpan.FromSeconds(5),
        LongBreakDuration = TimeSpan.FromSeconds(90),
    };

    public TimeSpan WorkDuration { get; init; } = TimeSpan.FromMinutes(25);
    public TimeSpan ShortBreakDuration { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan LongBreakDuration { get; init; } = TimeSpan.FromMinutes(90);
    public int WorkIntervalsBeforeLongBreak { get; init; } = 5;
}
