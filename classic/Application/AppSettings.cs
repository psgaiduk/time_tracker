using System;
using System.Collections.Generic;

namespace TimeTracker.Classic.Application
{
    internal sealed class AppSettings
    {
        internal AppSettings()
        {
            HideOverlayFromCapture = true;
            ShowOverlayOnAllVirtualDesktops = true;
            StartWithWindows = false;
            LongBreakEnabled = true;
            Monday = Tuesday = Wednesday = Thursday = Friday = Saturday = Sunday = true;
            WorkSummaryEnabled = true;
            WorkSummaryUrl = String.Empty;
            AutomaticMeetingApplications = new List<string>();
        }

        internal bool HideOverlayFromCapture { get; set; }
        internal bool ShowOverlayOnAllVirtualDesktops { get; set; }
        internal bool StartWithWindows { get; set; }
        internal bool LongBreakEnabled { get; set; }
        internal bool Monday { get; set; }
        internal bool Tuesday { get; set; }
        internal bool Wednesday { get; set; }
        internal bool Thursday { get; set; }
        internal bool Friday { get; set; }
        internal bool Saturday { get; set; }
        internal bool Sunday { get; set; }
        internal bool WorkSummaryEnabled { get; set; }
        internal string WorkSummaryUrl { get; set; }
        internal bool AutomaticMeetingEnabled { get; set; }
        internal List<string> AutomaticMeetingApplications { get; private set; }

        internal bool IsAutomaticMeetingApplication(string executablePath)
        {
            if (!AutomaticMeetingEnabled || String.IsNullOrEmpty(executablePath)) return false;
            foreach (string path in AutomaticMeetingApplications)
                if (String.Equals(path, executablePath, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        internal bool IsLongBreakAllowed(System.DateTime now)
        {
            if (!LongBreakEnabled) return false;
            switch (now.DayOfWeek)
            {
                case System.DayOfWeek.Monday: return Monday;
                case System.DayOfWeek.Tuesday: return Tuesday;
                case System.DayOfWeek.Wednesday: return Wednesday;
                case System.DayOfWeek.Thursday: return Thursday;
                case System.DayOfWeek.Friday: return Friday;
                case System.DayOfWeek.Saturday: return Saturday;
                default: return Sunday;
            }
        }
    }
}
