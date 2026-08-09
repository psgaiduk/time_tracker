using System;

namespace TimeTracker.Classic.Application
{
    internal sealed class DailyWorkStats
    {
        internal DailyWorkStats(TimeSpan currentPeriod, TimeSpan continuousWork, TimeSpan workedToday)
        {
            CurrentPeriod = currentPeriod;
            ContinuousWork = continuousWork;
            WorkedToday = workedToday;
        }

        internal TimeSpan CurrentPeriod { get; private set; }
        internal TimeSpan ContinuousWork { get; private set; }
        internal TimeSpan WorkedToday { get; private set; }
    }
}
