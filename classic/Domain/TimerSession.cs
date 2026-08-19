using System;

namespace TimeTracker.Classic.Domain
{
    internal sealed class TimerSession
    {
        private readonly TimerRules _rules;
        private TimerPhase _phase;
        private DateTime? _deadline;
        private DateTime? _workAccountingStartedAt;
        private DateTime? _breakStartedAt;
        private TimeSpan _breakDuration;
        private TimeSpan _shortBreakBalance;
        private TimeSpan _longBreakBalance;
        private int _completedWorkIntervals;

        internal TimerSession(TimerRules rules) : this(rules, TimeSpan.Zero, TimeSpan.Zero) { }

        internal TimerSession(TimerRules rules, TimeSpan shortBreakBalance, TimeSpan longBreakBalance)
        {
            _rules = rules;
            _phase = TimerPhase.Idle;
            _shortBreakBalance = NonNegative(shortBreakBalance);
            _longBreakBalance = NonNegative(longBreakBalance);
        }

        internal TimerState GetState(DateTime now)
        {
            TimeSpan remaining = TimeSpan.Zero;
            TimeSpan overdue = TimeSpan.Zero;
            if (_deadline.HasValue)
            {
                if (_deadline.Value > now) remaining = _deadline.Value - now;
                else if (IsBreak()) overdue = now - _deadline.Value;
            }
            return new TimerState(_phase, remaining, _completedWorkIntervals, CurrentShortBalance(now), CurrentLongBalance(now), overdue, _breakDuration);
        }

        internal void StartWork(DateTime now)
        {
            if (_phase != TimerPhase.Idle) throw new InvalidOperationException();
            _workAccountingStartedAt = now;
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
        }

        internal void Skip(DateTime now)
        {
            EnsureDecision();
            AccrueWork(now);
            _workAccountingStartedAt = now;
            Start(TimerPhase.Work, _rules.WorkDuration, now);
        }

        internal void Rest(DateTime now)
        {
            EnsureDecision();
            if (_rules.IsSummaryPromptEnabled())
            {
                _phase = TimerPhase.WorkSummary;
                _deadline = null;
            }
            else StartAccumulatedBreak(now);
        }

        internal void StartShortBreak(DateTime now)
        {
            if (_phase != TimerPhase.Work) throw new InvalidOperationException();
            StartAccumulatedBreak(now);
        }

        internal void CompleteWorkSummary(DateTime now)
        {
            if (_phase != TimerPhase.WorkSummary) throw new InvalidOperationException();
            StartAccumulatedBreak(now);
        }

        internal void EndBreak(DateTime now)
        {
            if (!IsBreak()) throw new InvalidOperationException();
            SettleBreak(now);
            _workAccountingStartedAt = now;
            Start(TimerPhase.Work, _rules.WorkDuration, now);
        }

        internal void Stop(DateTime now)
        {
            if (_phase == TimerPhase.Work || _phase == TimerPhase.AwaitingBreakDecision || _phase == TimerPhase.WorkSummary)
                AccrueWork(now);
            else if (IsBreak()) SettleBreak(now);
            _phase = TimerPhase.Idle;
            _deadline = null;
            _workAccountingStartedAt = null;
        }

        private void SettleBreak(DateTime now)
        {
            TimeSpan used = now > _breakStartedAt.Value ? now - _breakStartedAt.Value : TimeSpan.Zero;
            if (_phase == TimerPhase.ShortBreak)
            {
                _shortBreakBalance = NonNegative(_breakDuration - used);
                TimeSpan allowance = TimeSpan.FromTicks(_breakDuration.Ticks * 11 / 10);
                if (used > allowance) _longBreakBalance = NonNegative(_longBreakBalance - (used - allowance));
            }
            else _longBreakBalance = NonNegative(_breakDuration - used);
            _breakStartedAt = null;
            _breakDuration = TimeSpan.Zero;
        }

        private void StartAccumulatedBreak(DateTime now)
        {
            AccrueWork(now);
            bool useLong = _longBreakBalance >= _rules.LongBreakThreshold && _rules.IsLongBreakAllowed(now);
            if (useLong)
            {
                _shortBreakBalance = TimeSpan.Zero;
                BeginBreak(TimerPhase.LongBreak, _longBreakBalance, now);
            }
            else BeginBreak(TimerPhase.ShortBreak, _shortBreakBalance, now);
        }

        private void AccrueWork(DateTime now)
        {
            if (!_workAccountingStartedAt.HasValue) return;
            TimeSpan worked = now > _workAccountingStartedAt.Value ? now - _workAccountingStartedAt.Value : TimeSpan.Zero;
            _shortBreakBalance += _rules.AccrueShortBreak(worked);
            _longBreakBalance += _rules.AccrueLongBreak(worked);
            _workAccountingStartedAt = null;
        }

        private void BeginBreak(TimerPhase phase, TimeSpan duration, DateTime now)
        {
            _breakStartedAt = now;
            _breakDuration = duration;
            Start(phase, duration, now);
        }

        private TimeSpan CurrentShortBalance(DateTime now)
        {
            if (_phase != TimerPhase.ShortBreak || !_breakStartedAt.HasValue) return _shortBreakBalance;
            return NonNegative(_breakDuration - PositiveElapsed(now));
        }

        private TimeSpan CurrentLongBalance(DateTime now)
        {
            if (_phase != TimerPhase.LongBreak || !_breakStartedAt.HasValue) return _longBreakBalance;
            return NonNegative(_breakDuration - PositiveElapsed(now));
        }

        private TimeSpan PositiveElapsed(DateTime now)
        {
            return now > _breakStartedAt.Value ? now - _breakStartedAt.Value : TimeSpan.Zero;
        }

        private bool IsBreak() { return _phase == TimerPhase.ShortBreak || _phase == TimerPhase.LongBreak; }

        private void Start(TimerPhase phase, TimeSpan duration, DateTime now)
        {
            _phase = phase;
            _deadline = now.Add(duration);
        }

        private void EnsureDecision()
        {
            if (_phase != TimerPhase.AwaitingBreakDecision) throw new InvalidOperationException();
        }

        private static TimeSpan NonNegative(TimeSpan value) { return value < TimeSpan.Zero ? TimeSpan.Zero : value; }
    }
}
