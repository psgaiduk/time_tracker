using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class CsvWorkHistoryStore : IWorkHistoryStore
    {
        private readonly string _path;

        internal CsvWorkHistoryStore()
        {
            string data = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            Directory.CreateDirectory(data);
            _path = Path.Combine(data, "history.csv");
        }

        public void Add(HistoryEntry entry)
        {
            if (entry.FinishedAt <= entry.StartedAt) return;
            string line = entry.Kind + ";" + entry.StartedAt.ToString("o", CultureInfo.InvariantCulture) + ";" +
                entry.FinishedAt.ToString("o", CultureInfo.InvariantCulture) + ";" + entry.PlannedDuration.Ticks + ";" +
                entry.ShortBreakBalance.Ticks + ";" + entry.LongBreakBalance.Ticks + Environment.NewLine;
            File.AppendAllText(_path, line);
        }

        public TimeSpan GetTotal(DateTime day)
        {
            DateTime from = day.Date;
            DateTime to = from.AddDays(1);
            TimeSpan total = TimeSpan.Zero;
            foreach (HistoryEntry entry in ReadEntries())
            {
                if (entry.Kind != ActivityKind.Work) continue;
                DateTime segmentStart = entry.StartedAt < from ? from : entry.StartedAt;
                DateTime segmentFinish = entry.FinishedAt > to ? to : entry.FinishedAt;
                if (segmentFinish > segmentStart) total += segmentFinish - segmentStart;
            }
            return total;
        }

        public BreakBalances GetLatestBalances()
        {
            List<HistoryEntry> entries = ReadEntries();
            if (entries.Count == 0) return new BreakBalances(TimeSpan.Zero, TimeSpan.Zero);
            HistoryEntry latest = entries[entries.Count - 1];
            return new BreakBalances(latest.ShortBreakBalance, latest.LongBreakBalance);
        }

        private List<HistoryEntry> ReadEntries()
        {
            List<HistoryEntry> result = new List<HistoryEntry>();
            if (!File.Exists(_path)) return result;
            foreach (string line in File.ReadAllLines(_path))
            {
                string[] values = line.Split(';');
                DateTime start;
                DateTime finish;
                if (values.Length == 2 && TryDate(values[0], out start) && TryDate(values[1], out finish))
                {
                    result.Add(new HistoryEntry(ActivityKind.Work, start, finish, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero));
                    continue;
                }
                ActivityKind kind;
                long planned;
                long shortBalance;
                long longBalance;
                if (values.Length != 6 || !Enum.TryParse<ActivityKind>(values[0], out kind) || !TryDate(values[1], out start) ||
                    !TryDate(values[2], out finish) || !Int64.TryParse(values[3], out planned) ||
                    !Int64.TryParse(values[4], out shortBalance) || !Int64.TryParse(values[5], out longBalance)) continue;
                result.Add(new HistoryEntry(kind, start, finish, TimeSpan.FromTicks(planned), TimeSpan.FromTicks(shortBalance), TimeSpan.FromTicks(longBalance)));
            }
            return result;
        }

        private static bool TryDate(string value, out DateTime result)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
        }
    }
}
