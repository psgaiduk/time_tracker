using System;

namespace TimeTracker.Classic.Application
{
    internal interface IWorkHistoryStore
    {
        void Add(DateTime startedAt, DateTime finishedAt);
        TimeSpan GetTotal(DateTime day);
    }
}
