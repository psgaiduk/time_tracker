using System;
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

        public void Add(DateTime startedAt, DateTime finishedAt)
        {
            if (finishedAt <= startedAt) return;
            File.AppendAllText(_path, startedAt.ToString("o", CultureInfo.InvariantCulture) + ";" + finishedAt.ToString("o", CultureInfo.InvariantCulture) + Environment.NewLine);
        }

        public TimeSpan GetTotal(DateTime day)
        {
            if (!File.Exists(_path)) return TimeSpan.Zero;
            DateTime from = day.Date;
            DateTime to = from.AddDays(1);
            TimeSpan total = TimeSpan.Zero;
            foreach (string line in File.ReadAllLines(_path))
            {
                string[] values = line.Split(';');
                DateTime start;
                DateTime finish;
                if (values.Length != 2 || !DateTime.TryParse(values[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out start) || !DateTime.TryParse(values[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out finish)) continue;
                DateTime segmentStart = start < from ? from : start;
                DateTime segmentFinish = finish > to ? to : finish;
                if (segmentFinish > segmentStart) total += segmentFinish - segmentStart;
            }
            return total;
        }
    }
}
