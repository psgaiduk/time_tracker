using System;

namespace TimeTracker.Classic.Domain
{
    internal sealed class TimerRules
    {
        private readonly Func<DateTime, bool> _longBreakAllowed;
        private readonly Func<bool> _summaryPromptEnabled;

        internal TimerRules(TimeSpan work, TimeSpan shortBreak, TimeSpan longBreak)
            : this(work, shortBreak, longBreak, delegate(DateTime date) { return true; }, delegate { return false; })
        {
        }

        internal TimerRules(TimeSpan work, TimeSpan shortBreak, TimeSpan longBreak, Func<DateTime, bool> longBreakAllowed)
            : this(work, shortBreak, longBreak, longBreakAllowed, delegate { return false; })
        {
        }

        internal TimerRules(TimeSpan work, TimeSpan shortBreak, TimeSpan longBreak, Func<DateTime, bool> longBreakAllowed, Func<bool> summaryPromptEnabled)
        {
            WorkDuration = work;
            ShortBreakDuration = shortBreak;
            LongBreakDuration = longBreak;
            _longBreakAllowed = longBreakAllowed;
            _summaryPromptEnabled = summaryPromptEnabled;
        }

        internal TimeSpan WorkDuration { get; private set; }
        internal TimeSpan ShortBreakDuration { get; private set; }
        internal TimeSpan LongBreakDuration { get; private set; }
        internal int WorkIntervalsBeforeLongBreak { get { return 5; } }

        internal static TimerRules Default()
        {
            return new TimerRules(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(90));
        }

        internal static TimerRules Default(Func<DateTime, bool> longBreakAllowed)
        {
            return new TimerRules(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(90), longBreakAllowed);
        }

        internal static TimerRules Default(Func<DateTime, bool> longBreakAllowed, Func<bool> summaryPromptEnabled)
        {
            return new TimerRules(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(90), longBreakAllowed, summaryPromptEnabled);
        }

        internal static TimerRules Test()
        {
            return new TimerRules(TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(90));
        }

        internal static TimerRules Test(Func<DateTime, bool> longBreakAllowed)
        {
            return new TimerRules(TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(90), longBreakAllowed);
        }

        internal static TimerRules Test(Func<DateTime, bool> longBreakAllowed, Func<bool> summaryPromptEnabled)
        {
            return new TimerRules(TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(90), longBreakAllowed, summaryPromptEnabled);
        }

        internal bool IsLongBreakAllowed(DateTime now) { return _longBreakAllowed(now); }
        internal bool IsSummaryPromptEnabled() { return _summaryPromptEnabled(); }
    }
}
