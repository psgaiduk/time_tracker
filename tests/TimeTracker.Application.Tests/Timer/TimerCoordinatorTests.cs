using TimeTracker.Application.Ports;
using TimeTracker.Application.Timer;
using TimeTracker.Domain.Timer;

namespace TimeTracker.Application.Tests.Timer;

public sealed class TimerCoordinatorTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Tick_UsesClockDeadlineInsteadOfTickCount()
    {
        var clock = new FakeClock(Start);
        var coordinator = new TimerCoordinator(clock);

        coordinator.Start();
        clock.Advance(TimeSpan.FromMinutes(24).Add(TimeSpan.FromSeconds(59)));
        coordinator.Tick();

        Assert.Equal(TimerPhase.Work, coordinator.State.Phase);

        clock.Advance(TimeSpan.FromSeconds(1));
        coordinator.Tick();

        Assert.Equal(TimerPhase.AwaitingBreakDecision, coordinator.State.Phase);
    }

    private sealed class FakeClock : IClock
    {
        public FakeClock(DateTimeOffset now) => Now = now;

        public DateTimeOffset Now { get; private set; }

        public void Advance(TimeSpan duration) => Now = Now.Add(duration);
    }
}
