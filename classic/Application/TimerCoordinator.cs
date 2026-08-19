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
        private DateTime? _breakStartedAt;
        private TimerPhase _breakPhase;
        private TimeSpan _plannedBreakDuration;

        internal TimerCoordinator(IClock clock, TimerRules rules, IWorkHistoryStore history)
        {
            _clock = clock;
            _history = history;
            BreakBalances balances = history.GetLatestBalances();
            _session = new TimerSession(rules, balances.ShortBreak, balances.LongBreak);
            Publish();
        }

        internal TimerState State { get; private set; }
        internal event EventHandler StateChanged;
        internal DailyWorkStats Stats { get; private set; }

        internal void Start() { DateTime now = _clock.Now; _session.StartWork(now); BeginWork(now, true); Publish(); }
        internal void Tick() { _session.Advance(_clock.Now); Publish(); }

        internal void Skip()
        {
            DateTime now = _clock.Now;
            _session.Skip(now);
            RecordAndRestartWork(now, false);
            Publish();
        }

        internal void Rest()
        {
            DateTime now = _clock.Now;
            _session.Rest(now);
            TimerState state = _session.GetState(now);
            if (IsBreak(state.Phase))
            {
                EndWork(now, true, state);
                BeginBreak(now, state);
            }
            Publish();
        }

        internal void StartShortBreak()
        {
            DateTime now = _clock.Now;
            _session.StartShortBreak(now);
            TimerState state = _session.GetState(now);
            EndWork(now, true, state);
            BeginBreak(now, state);
            Publish();
        }

        internal void EndBreak()
        {
            DateTime now = _clock.Now;
            DateTime? startedAt = _breakStartedAt;
            TimerPhase phase = _breakPhase;
            TimeSpan planned = _plannedBreakDuration;
            _session.EndBreak(now);
            TimerState state = _session.GetState(now);
            if (startedAt.HasValue)
            {
                ActivityKind kind = phase == TimerPhase.LongBreak ? ActivityKind.LongBreak : ActivityKind.ShortBreak;
                _history.Add(new HistoryEntry(kind, startedAt.Value, now, planned, state.ShortBreakBalance, state.LongBreakBalance));
            }
            _breakStartedAt = null;
            BeginWork(now, true);
            Publish();
        }

        internal void CompleteWorkSummary()
        {
            DateTime now = _clock.Now;
            _session.CompleteWorkSummary(now);
            TimerState state = _session.GetState(now);
            EndWork(now, true, state);
            BeginBreak(now, state);
            Publish();
        }

        internal void Stop()
        {
            DateTime now = _clock.Now;
            bool wasBreak = IsBreak(_session.GetState(now).Phase);
            _session.Stop(now);
            TimerState state = _session.GetState(now);
            if (wasBreak && _breakStartedAt.HasValue)
            {
                ActivityKind kind = _breakPhase == TimerPhase.LongBreak ? ActivityKind.LongBreak : ActivityKind.ShortBreak;
                _history.Add(new HistoryEntry(kind, _breakStartedAt.Value, now, _plannedBreakDuration, state.ShortBreakBalance, state.LongBreakBalance));
            }
            else EndWork(now, true, state);
            _breakStartedAt = null;
            _currentPeriodStartedAt = null;
            _continuousWorkStartedAt = null;
            Publish();
        }

        private void Publish()
        {
            DateTime now = _clock.Now;
            State = _session.GetState(now);
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

        private void RecordAndRestartWork(DateTime now, bool beginContinuous)
        {
            EndWork(now, false, _session.GetState(now));
            BeginWork(now, beginContinuous);
        }

        private void EndWork(DateTime now, bool endContinuous, TimerState state)
        {
            if (_currentPeriodStartedAt.HasValue)
                _history.Add(new HistoryEntry(ActivityKind.Work, _currentPeriodStartedAt.Value, now, TimeSpan.Zero, state.ShortBreakBalance, state.LongBreakBalance));
            _currentPeriodStartedAt = null;
            if (endContinuous) _continuousWorkStartedAt = null;
        }

        private void BeginBreak(DateTime now, TimerState state)
        {
            _breakStartedAt = now;
            _breakPhase = state.Phase;
            _plannedBreakDuration = state.BreakDuration;
        }

        private static bool IsBreak(TimerPhase phase)
        {
            return phase == TimerPhase.ShortBreak || phase == TimerPhase.LongBreak;
        }
    }
}
