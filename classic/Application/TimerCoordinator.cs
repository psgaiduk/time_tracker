using System;
using TimeTracker.Classic.Domain;

namespace TimeTracker.Classic.Application
{
    internal sealed class TimerCoordinator
    {
        private readonly IClock _clock;
        private readonly TimerSession _session;
        private readonly IWorkHistoryStore _history;
        private DateTime? _currentPeriodStartedAt;
        private DateTime? _continuousWorkStartedAt;

        internal TimerCoordinator(IClock clock, TimerRules rules, IWorkHistoryStore history)
        {
            _clock = clock;
            _history = history;
            _session = new TimerSession(rules);
            Publish();
        }

        internal TimerState State { get; private set; }
        internal event EventHandler StateChanged;
        internal DailyWorkStats Stats { get; private set; }

        internal void Start() { _session.StartWork(_clock.Now); BeginWork(_clock.Now, true); Publish(); }
        internal void Tick()
        {
            TimerPhase before = State.Phase;
            _session.Advance(_clock.Now);
            TimerPhase after = _session.GetState(_clock.Now).Phase;
            if (before == TimerPhase.Work && after == TimerPhase.LongBreak) EndWork(_clock.Now, true);
            if ((before == TimerPhase.ShortBreak || before == TimerPhase.LongBreak) && after == TimerPhase.Work) BeginWork(_clock.Now, true);
            Publish();
        }
        internal void Skip() { EndWork(_clock.Now, false); _session.Skip(_clock.Now); BeginWork(_clock.Now, false); Publish(); }
        internal void Rest() { EndWork(_clock.Now, true); _session.Rest(_clock.Now); Publish(); }
        internal void EndBreak() { _session.EndBreak(_clock.Now); BeginWork(_clock.Now, true); Publish(); }

        private void Publish()
        {
            State = _session.GetState(_clock.Now);
            DateTime now = _clock.Now;
            TimeSpan current = _currentPeriodStartedAt.HasValue ? now - _currentPeriodStartedAt.Value : TimeSpan.Zero;
            TimeSpan continuous = _continuousWorkStartedAt.HasValue ? now - _continuousWorkStartedAt.Value : TimeSpan.Zero;
            TimeSpan today = _history.GetTotal(now.Date);
            if (_currentPeriodStartedAt.HasValue)
                today += now - (_currentPeriodStartedAt.Value < now.Date ? now.Date : _currentPeriodStartedAt.Value);
            Stats = new DailyWorkStats(current, continuous, today);
            EventHandler handler = StateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void BeginWork(DateTime now, bool beginContinuous)
        {
            _currentPeriodStartedAt = now;
            if (beginContinuous || !_continuousWorkStartedAt.HasValue) _continuousWorkStartedAt = now;
        }

        private void EndWork(DateTime now, bool endContinuous)
        {
            if (_currentPeriodStartedAt.HasValue) _history.Add(_currentPeriodStartedAt.Value, now);
            _currentPeriodStartedAt = null;
            if (endContinuous) _continuousWorkStartedAt = null;
        }
    }
}
