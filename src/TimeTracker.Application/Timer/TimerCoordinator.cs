using TimeTracker.Application.Ports;
using TimeTracker.Domain.Timer;

namespace TimeTracker.Application.Timer;

public sealed class TimerCoordinator
{
    private readonly IClock _clock;
    private readonly TimerSession _session;

    public TimerCoordinator(IClock clock, TimerRules? rules = null)
    {
        _clock = clock;
        _session = new TimerSession(rules);
        State = _session.GetState(_clock.Now);
    }

    public TimerState State { get; private set; }

    public event EventHandler<TimerState>? StateChanged;

    public void Start()
    {
        _session.StartWork(_clock.Now);
        Publish();
    }

    public void Tick()
    {
        _session.Advance(_clock.Now);
        Publish();
    }

    public void Skip()
    {
        _session.SkipToNextWork(_clock.Now);
        Publish();
    }

    public void Rest()
    {
        _session.TakeShortBreak(_clock.Now);
        Publish();
    }

    private void Publish()
    {
        State = _session.GetState(_clock.Now);
        StateChanged?.Invoke(this, State);
    }
}
