using System;
using System.Collections.Generic;
using System.Diagnostics;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class WindowsApplicationCatalog : IApplicationCatalog
    {
        public IList<ApplicationChoice> GetRunningApplications()
        {
            List<ApplicationChoice> result = new List<ApplicationChoice>();
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainWindowHandle == IntPtr.Zero || String.IsNullOrWhiteSpace(process.MainWindowTitle)) continue;
                    result.Add(new ApplicationChoice(process.MainWindowTitle, process.MainModule.FileName));
                }
                catch (Exception) { }
                finally { process.Dispose(); }
            }
            result.Sort(delegate(ApplicationChoice left, ApplicationChoice right) { return String.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase); });
            return result;
        }
    }
}
