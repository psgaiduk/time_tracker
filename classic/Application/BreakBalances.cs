using System;

namespace TimeTracker.Classic.Application
{
    internal sealed class BreakBalances
    {
        internal BreakBalances(TimeSpan shortBreak, TimeSpan longBreak)
        {
            ShortBreak = shortBreak;
            LongBreak = longBreak;
        }

        internal TimeSpan ShortBreak { get; private set; }
        internal TimeSpan LongBreak { get; private set; }

        internal BreakBalances AfterUntrackedRest(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero) return this;
            return new BreakBalances(Subtract(ShortBreak, duration), Subtract(LongBreak, duration));
        }

        private static TimeSpan Subtract(TimeSpan balance, TimeSpan duration)
        {
            return balance > duration ? balance - duration : TimeSpan.Zero;
        }
    }
}
