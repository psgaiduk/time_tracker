namespace TimeTracker.Domain.Timer;

public sealed class TimerSession
{
    private readonly TimerRules _rules;
    private TimerPhase _phase = TimerPhase.Idle;
    private DateTimeOffset? _deadline;
    private int _completedWorkIntervals;

    public TimerSession(TimerRules? rules = null)
    {
        _rules = rules ?? TimerRules.Default;
    }

    public TimerState GetState(DateTimeOffset now)
    {
        var remaining = _deadline is null
            ? TimeSpan.Zero
            : _deadline.Value > now ? _deadline.Value - now : TimeSpan.Zero;

        return new TimerState(_phase, remaining, _completedWorkIntervals, _deadline);
    }

    public void StartWork(DateTimeOffset now)
    {
        EnsurePhase(TimerPhase.Idle);
        StartPhase(TimerPhase.Work, _rules.WorkDuration, now);
    }

    public void Advance(DateTimeOffset now)
    {
        if (_deadline is null || now < _deadline.Value)
        {
            return;
        }

        switch (_phase)
        {
            case TimerPhase.Work:
                CompleteWork(now);
                break;
            case TimerPhase.ShortBreak:
                StartPhase(TimerPhase.Work, _rules.WorkDuration, now);
                break;
            case TimerPhase.LongBreak:
                StartPhase(TimerPhase.Work, _rules.WorkDuration, now);
                break;
        }
    }

    public void SkipToNextWork(DateTimeOffset now)
    {
        EnsurePhase(TimerPhase.AwaitingBreakDecision);
        StartPhase(TimerPhase.Work, _rules.WorkDuration, now);
    }

    public void TakeShortBreak(DateTimeOffset now)
    {
        EnsurePhase(TimerPhase.AwaitingBreakDecision);
        StartPhase(TimerPhase.ShortBreak, _rules.ShortBreakDuration, now);
    }

    private void CompleteWork(DateTimeOffset now)
    {
        _completedWorkIntervals++;

        if (_completedWorkIntervals >= _rules.WorkIntervalsBeforeLongBreak)
        {
            _completedWorkIntervals = 0;
            StartPhase(TimerPhase.LongBreak, _rules.LongBreakDuration, now);
            return;
        }

        _phase = TimerPhase.AwaitingBreakDecision;
        _deadline = null;
    }

    private void StartPhase(TimerPhase phase, TimeSpan duration, DateTimeOffset now)
    {
        _phase = phase;
        _deadline = now + duration;
    }

    private void EnsurePhase(TimerPhase expected)
    {
        if (_phase != expected)
        {
            throw new InvalidOperationException($"Action is not available during {_phase} phase.");
        }
    }
}
