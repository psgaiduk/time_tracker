using System;

namespace TimeTracker.Classic.Domain
{
    internal sealed class TimerSession
    {
        private readonly TimerRules _rules;
        private TimerPhase _phase;
        private DateTime? _deadline;
        private int _completedWorkIntervals;

        internal TimerSession(TimerRules rules)
        {
            _rules = rules;
            _phase = TimerPhase.Idle;
        }

        internal TimerState GetState(DateTime now)
        {
            TimeSpan remaining = TimeSpan.Zero;
            if (_deadline.HasValue && _deadline.Value > now)
                remaining = _deadline.Value - now;
            return new TimerState(_phase, remaining, _completedWorkIntervals);
        }

        internal void StartWork(DateTime now)
        {
            if (_phase != TimerPhase.Idle) throw new InvalidOperationException();
            Start(TimerPhase.Work, _rules.WorkDuration, now);
        }

        internal void Advance(DateTime now)
        {
            if (!_deadline.HasValue || now < _deadline.Value) return;
            if (_phase == TimerPhase.Work)
            {
                _completedWorkIntervals++;
                _phase = TimerPhase.AwaitingBreakDecision;
                _deadline = null;
            }
            else if (_phase == TimerPhase.ShortBreak || _phase == TimerPhase.LongBreak)
            {
                Start(TimerPhase.Work, _rules.WorkDuration, now);
            }
        }

        internal void Skip(DateTime now)
        {
            EnsureDecision();
            Start(TimerPhase.Work, _rules.WorkDuration, now);
        }

        internal void Rest(DateTime now)
        {
            EnsureDecision();
            if (_completedWorkIntervals >= _rules.WorkIntervalsBeforeLongBreak && _rules.IsLongBreakAllowed(now))
            {
                _completedWorkIntervals = 0;
                Start(TimerPhase.LongBreak, _rules.LongBreakDuration, now);
            }
            else
            {
                Start(TimerPhase.ShortBreak, _rules.ShortBreakDuration, now);
            }
        }

        internal void EndBreak(DateTime now)
        {
            if (_phase != TimerPhase.ShortBreak && _phase != TimerPhase.LongBreak)
                throw new InvalidOperationException();
            Start(TimerPhase.Work, _rules.WorkDuration, now);
        }

        private void Start(TimerPhase phase, TimeSpan duration, DateTime now)
        {
            _phase = phase;
            _deadline = now.Add(duration);
        }

        private void EnsureDecision()
        {
            if (_phase != TimerPhase.AwaitingBreakDecision) throw new InvalidOperationException();
        }
    }
}
