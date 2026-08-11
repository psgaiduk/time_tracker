using System;
using System.IO;
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
                bool value;
                if (!Boolean.TryParse(pair[1], out value)) continue;
                if (pair[0] == "HideOverlayFromCapture") result.HideOverlayFromCapture = value;
                if (pair[0] == "ShowOverlayOnAllVirtualDesktops") result.ShowOverlayOnAllVirtualDesktops = value;
                if (pair[0] == "StartWithWindows") result.StartWithWindows = value;
                if (pair[0] == "LongBreakEnabled") result.LongBreakEnabled = value;
                if (pair[0] == "Monday") result.Monday = value;
                if (pair[0] == "Tuesday") result.Tuesday = value;
                if (pair[0] == "Wednesday") result.Wednesday = value;
                if (pair[0] == "Thursday") result.Thursday = value;
                if (pair[0] == "Friday") result.Friday = value;
                if (pair[0] == "Saturday") result.Saturday = value;
                if (pair[0] == "Sunday") result.Sunday = value;
                if (pair[0] == "WorkSummaryEnabled") result.WorkSummaryEnabled = value;
            }
            return result;
        }

        public void Save(AppSettings settings)
        {
            File.WriteAllLines(_path, new[] {
                "HideOverlayFromCapture=" + settings.HideOverlayFromCapture,
                "ShowOverlayOnAllVirtualDesktops=" + settings.ShowOverlayOnAllVirtualDesktops,
                "StartWithWindows=" + settings.StartWithWindows,
                "LongBreakEnabled=" + settings.LongBreakEnabled,
                "Monday=" + settings.Monday,
                "Tuesday=" + settings.Tuesday,
                "Wednesday=" + settings.Wednesday,
                "Thursday=" + settings.Thursday,
                "Friday=" + settings.Friday,
                "Saturday=" + settings.Saturday,
                "Sunday=" + settings.Sunday,
                "WorkSummaryEnabled=" + settings.WorkSummaryEnabled,
                "WorkSummaryUrl=" + (settings.WorkSummaryUrl ?? String.Empty)
            });
        }
    }
}
