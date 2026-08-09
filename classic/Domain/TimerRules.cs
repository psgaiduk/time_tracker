using System;

namespace TimeTracker.Classic.Domain
{
    internal sealed class TimerRules
    {
        internal TimerRules(TimeSpan work, TimeSpan shortBreak, TimeSpan longBreak)
        {
            WorkDuration = work;
            ShortBreakDuration = shortBreak;
            LongBreakDuration = longBreak;
        }

        internal TimeSpan WorkDuration { get; private set; }
        internal TimeSpan ShortBreakDuration { get; private set; }
        internal TimeSpan LongBreakDuration { get; private set; }
        internal int WorkIntervalsBeforeLongBreak { get { return 5; } }

        internal static TimerRules Default()
        {
            return new TimerRules(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(90));
        }

        internal static TimerRules Test()
        {
            return new TimerRules(TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(90));
        }
    }
}
