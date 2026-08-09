using TimeTracker.Domain.Timer;

namespace TimeTracker.Domain.Tests.Timer;

public sealed class TimerSessionTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TestRules_UseSecondsInsteadOfMinutes()
    {
        Assert.Equal(TimeSpan.FromSeconds(25), TimerRules.Test.WorkDuration);
        Assert.Equal(TimeSpan.FromSeconds(5), TimerRules.Test.ShortBreakDuration);
        Assert.Equal(TimeSpan.FromSeconds(90), TimerRules.Test.LongBreakDuration);
        Assert.Equal(5, TimerRules.Test.WorkIntervalsBeforeLongBreak);
    }

    [Fact]
    public void StartWork_CreatesTwentyFiveMinuteWorkInterval()
    {
        var session = new TimerSession();

        session.StartWork(Start);

        var state = session.GetState(Start);
        Assert.Equal(TimerPhase.Work, state.Phase);
        Assert.Equal(TimeSpan.FromMinutes(25), state.Remaining);
    }

    [Fact]
    public void WorkCompletion_BeforeFifthInterval_WaitsForBreakDecision()
    {
        var session = new TimerSession();
        session.StartWork(Start);

        session.Advance(Start.AddMinutes(25));

        var state = session.GetState(Start.AddMinutes(25));
        Assert.Equal(TimerPhase.AwaitingBreakDecision, state.Phase);
        Assert.Equal(1, state.CompletedWorkIntervals);
        Assert.Equal(TimeSpan.Zero, state.Remaining);
    }

    [Fact]
    public void SkipAfterWorkCompletion_StartsAnotherWorkInterval()
    {
        var session = new TimerSession();
        session.StartWork(Start);
        session.Advance(Start.AddMinutes(25));

        session.SkipToNextWork(Start.AddMinutes(25));

        var state = session.GetState(Start.AddMinutes(25));
        Assert.Equal(TimerPhase.Work, state.Phase);
        Assert.Equal(TimeSpan.FromMinutes(25), state.Remaining);
        Assert.Equal(1, state.CompletedWorkIntervals);
    }

    [Fact]
    public void TakingShortBreak_StartsFiveMinuteBreak()
    {
        var session = new TimerSession();
        session.StartWork(Start);
        session.Advance(Start.AddMinutes(25));

        session.TakeShortBreak(Start.AddMinutes(25));

        var state = session.GetState(Start.AddMinutes(25));
        Assert.Equal(TimerPhase.ShortBreak, state.Phase);
        Assert.Equal(TimeSpan.FromMinutes(5), state.Remaining);
    }

    [Fact]
    public void FifthCompletedWorkInterval_StartsNinetyMinuteLongBreakAndResetsCount()
    {
        var session = new TimerSession();
        var now = Start;
        session.StartWork(now);

        for (var interval = 0; interval < 5; interval++)
        {
            now = now.AddMinutes(25);
            session.Advance(now);

            if (interval < 4)
            {
                session.TakeShortBreak(now);
                now = now.AddMinutes(5);
                session.Advance(now);
            }
        }

        var state = session.GetState(now);
        Assert.Equal(TimerPhase.LongBreak, state.Phase);
        Assert.Equal(TimeSpan.FromMinutes(90), state.Remaining);
        Assert.Equal(0, state.CompletedWorkIntervals);
    }

    [Fact]
    public void TimerDoesNotCompleteBeforeDeadline()
    {
        var session = new TimerSession();
        session.StartWork(Start);

        session.Advance(Start.AddMinutes(24).AddSeconds(59));

        Assert.Equal(TimerPhase.Work, session.GetState(Start.AddMinutes(24).AddSeconds(59)).Phase);
    }
}
