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
        private ActivityKind _currentActivityKind;

        internal TimerCoordinator(IClock clock, TimerRules rules, IWorkHistoryStore history)
        {
            _clock = clock;
            _history = history;
            BreakBalances balances = history.GetLatestBalances();
            DateTime? latestFinishedAt = history.GetLatestFinishedAt();
            if (latestFinishedAt.HasValue && clock.Now > latestFinishedAt.Value)
                balances = balances.AfterUntrackedRest(clock.Now - latestFinishedAt.Value);
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

        internal void ToggleMeeting()
        {
            DateTime now = _clock.Now;
            if (_session.GetState(now).Phase == TimerPhase.Work)
            {
                _session.StartMeeting(now);
                SwitchActivity(now, ActivityKind.Meeting);
            }
            else if (_session.GetState(now).Phase == TimerPhase.Meeting)
            {
                _session.EndMeeting(now);
                SwitchActivity(now, ActivityKind.Work);
            }
            else throw new InvalidOperationException();
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

        internal WorkDaySummary FinishWorkDay()
        {
            DateTime now = _clock.Now;
            Stop();
            return WorkDaySummary.Create(_history.GetEntries(now.Date), now);
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
            _currentActivityKind = ActivityKind.Work;
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
                _history.Add(new HistoryEntry(_currentActivityKind, _currentPeriodStartedAt.Value, now, TimeSpan.Zero, state.ShortBreakBalance, state.LongBreakBalance));
            _currentPeriodStartedAt = null;
            if (endContinuous) _continuousWorkStartedAt = null;
        }

        private void SwitchActivity(DateTime now, ActivityKind next)
        {
            EndWork(now, false, _session.GetState(now));
            _currentPeriodStartedAt = now;
            _currentActivityKind = next;
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
