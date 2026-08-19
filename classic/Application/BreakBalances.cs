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
    }
}
