using System;

namespace TimeTracker.Classic.Application
{
    internal interface IWorkHistoryStore
    {
        void Add(HistoryEntry entry);
        TimeSpan GetTotal(DateTime day);
        BreakBalances GetLatestBalances();
    }
}
