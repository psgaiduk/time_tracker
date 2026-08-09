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
                bool value;
                if (!Boolean.TryParse(pair[1], out value)) continue;
                if (pair[0] == "HideOverlayFromCapture") result.HideOverlayFromCapture = value;
                if (pair[0] == "StartWithWindows") result.StartWithWindows = value;
            }
            return result;
        }

        public void Save(AppSettings settings)
        {
            File.WriteAllLines(_path, new[] {
                "HideOverlayFromCapture=" + settings.HideOverlayFromCapture,
                "StartWithWindows=" + settings.StartWithWindows
            });
        }
    }
}
