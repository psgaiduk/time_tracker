using System;
using System.Collections.Generic;

namespace TimeTracker.Classic.Application
{
    internal interface IWorkHistoryStore
    {
        void Add(HistoryEntry entry);
        TimeSpan GetTotal(DateTime day);
        BreakBalances GetLatestBalances();
        IList<HistoryEntry> GetEntries(DateTime day);
    }
}
