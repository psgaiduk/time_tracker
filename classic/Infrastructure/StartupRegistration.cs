using System;
using Microsoft.Win32;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class StartupRegistration
    {
        private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "TimeTracker";

        internal void SetEnabled(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath, true))
            {
                if (key == null) return;
                if (enabled)
                    key.SetValue(ValueName, "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                else
                    key.DeleteValue(ValueName, false);
            }
        }
    }
}
