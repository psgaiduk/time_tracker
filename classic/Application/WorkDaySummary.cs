using System;
using System.Collections.Generic;

namespace TimeTracker.Classic.Application
{
    internal sealed class WorkDaySummary
    {
        private WorkDaySummary(DateTime startedAt, DateTime finishedAt, TimeSpan workDuration, TimeSpan meetingDuration, TimeSpan shortBreakDuration, TimeSpan longBreakDuration, IList<HistoryEntry> entries)
        {
            StartedAt = startedAt;
            FinishedAt = finishedAt;
            WorkDuration = workDuration;
            MeetingDuration = meetingDuration;
            ShortBreakDuration = shortBreakDuration;
            LongBreakDuration = longBreakDuration;
            Entries = entries;
        }

        internal DateTime StartedAt { get; private set; }
        internal DateTime FinishedAt { get; private set; }
        internal TimeSpan TotalDuration { get { return FinishedAt - StartedAt; } }
        internal TimeSpan WorkDuration { get; private set; }
        internal TimeSpan MeetingDuration { get; private set; }
        internal TimeSpan TotalWorkDuration { get { return WorkDuration + MeetingDuration; } }
        internal TimeSpan ShortBreakDuration { get; private set; }
        internal TimeSpan LongBreakDuration { get; private set; }
        internal TimeSpan BreakDuration { get { return ShortBreakDuration + LongBreakDuration; } }
        internal IList<HistoryEntry> Entries { get; private set; }
        internal bool HasEntries { get { return Entries.Count > 0; } }

        internal static WorkDaySummary Create(IList<HistoryEntry> source, DateTime finishedAt)
        {
            List<HistoryEntry> entries = new List<HistoryEntry>(source);
            entries.Sort(delegate(HistoryEntry left, HistoryEntry right) { return left.StartedAt.CompareTo(right.StartedAt); });
            DateTime startedAt = entries.Count == 0 ? finishedAt : entries[0].StartedAt;
            TimeSpan work = TimeSpan.Zero;
            TimeSpan meetings = TimeSpan.Zero;
            TimeSpan shortBreaks = TimeSpan.Zero;
            TimeSpan longBreaks = TimeSpan.Zero;
            foreach (HistoryEntry entry in entries)
            {
                TimeSpan duration = entry.FinishedAt - entry.StartedAt;
                if (entry.Kind == ActivityKind.Work) work += duration;
                else if (entry.Kind == ActivityKind.Meeting) meetings += duration;
                else if (entry.Kind == ActivityKind.ShortBreak) shortBreaks += duration;
                else if (entry.Kind == ActivityKind.LongBreak) longBreaks += duration;
            }
            return new WorkDaySummary(startedAt, finishedAt, work, meetings, shortBreaks, longBreaks, entries.AsReadOnly());
        }
    }
}
