using System;
using System.IO;
using System.Collections.Generic;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class PortableSettingsStore : ISettingsStore
    {
        private readonly string _path;

        internal PortableSettingsStore()
        {
            string data = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            Directory.CreateDirectory(data);
            _path = Path.Combine(data, "settings.ini");
        }

        public AppSettings Load()
        {
            AppSettings result = new AppSettings();
            if (!File.Exists(_path)) return result;
            foreach (string line in File.ReadAllLines(_path))
            {
                string[] pair = line.Split(new[] { '=' }, 2);
                if (pair.Length != 2) continue;
                if (pair[0] == "WorkSummaryUrl") { result.WorkSummaryUrl = pair[1]; continue; }
                TimeSpan time;
                if (pair[0] == "WorkDayStart" && TimeSpan.TryParse(pair[1], out time)) { result.WorkDayStart = time; continue; }
                if (pair[0] == "WorkDayEnd" && TimeSpan.TryParse(pair[1], out time)) { result.WorkDayEnd = time; continue; }
                if (pair[0] == "AutomaticMeetingApplication") { if (!String.IsNullOrWhiteSpace(pair[1])) result.AutomaticMeetingApplications.Add(pair[1]); continue; }
                bool value;
                if (!Boolean.TryParse(pair[1], out value)) continue;
                if (pair[0] == "HideOverlayFromCapture") result.HideOverlayFromCapture = value;
                if (pair[0] == "ShowOverlayOnAllVirtualDesktops") result.ShowOverlayOnAllVirtualDesktops = value;
                if (pair[0] == "StartWithWindows") result.StartWithWindows = value;
                if (pair[0] == "WorkSummaryEnabled") result.WorkSummaryEnabled = value;
                if (pair[0] == "AutomaticMeetingEnabled") result.AutomaticMeetingEnabled = value;
            }
            return result;
        }

        public void Save(AppSettings settings)
        {
            List<string> lines = new List<string>(new[] {
                "HideOverlayFromCapture=" + settings.HideOverlayFromCapture,
                "ShowOverlayOnAllVirtualDesktops=" + settings.ShowOverlayOnAllVirtualDesktops,
                "StartWithWindows=" + settings.StartWithWindows,
                "WorkDayStart=" + settings.WorkDayStart,
                "WorkDayEnd=" + settings.WorkDayEnd,
                "WorkSummaryEnabled=" + settings.WorkSummaryEnabled,
                "WorkSummaryUrl=" + (settings.WorkSummaryUrl ?? String.Empty),
                "AutomaticMeetingEnabled=" + settings.AutomaticMeetingEnabled
            });
            foreach (string path in settings.AutomaticMeetingApplications) lines.Add("AutomaticMeetingApplication=" + path);
            File.WriteAllLines(_path, lines.ToArray());
        }
    }
}
