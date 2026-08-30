using System;

namespace TimeTracker.Classic.Application
{
    internal static class WorkDayReportText
    {
        internal static string Format(WorkDaySummary summary)
        {
            return "Активная работа: " + FormatDuration(summary.WorkDuration) + "\r\n" +
                "Время на звонках: " + FormatDuration(summary.MeetingDuration) + "\r\n" +
                "Начало рабочего дня: " + summary.StartedAt.ToString("dd.MM.yyyy HH:mm") + "\r\n" +
                "Конец рабочего дня: " + summary.FinishedAt.ToString("dd.MM.yyyy HH:mm");
        }

        private static string FormatDuration(TimeSpan value)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", (int)value.TotalHours, value.Minutes, value.Seconds);
        }
    }
}
