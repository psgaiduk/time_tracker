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
            WorkDayStart = TimeSpan.FromHours(9);
            WorkDayEnd = TimeSpan.FromHours(18);
            WorkSummaryEnabled = true;
            WorkSummaryUrl = String.Empty;
            AutomaticMeetingApplications = new List<string>();
        }

        internal bool HideOverlayFromCapture { get; set; }
        internal bool ShowOverlayOnAllVirtualDesktops { get; set; }
        internal bool StartWithWindows { get; set; }
        internal TimeSpan WorkDayStart { get; set; }
        internal TimeSpan WorkDayEnd { get; set; }
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

    }
}
