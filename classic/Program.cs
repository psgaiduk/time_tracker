using System;
using System.Windows.Forms;
using TimeTracker.Classic.Application;
using TimeTracker.Classic.Domain;
using TimeTracker.Classic.Infrastructure;
using TimeTracker.Classic.Presentation;

namespace TimeTracker.Classic
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            ISettingsStore settingsStore = new PortableSettingsStore();
            AppSettings settings = settingsStore.Load();
#if TEST_TIMER
            TimerRules rules = TimerRules.Test(settings.IsLongBreakAllowed, delegate { return settings.WorkSummaryEnabled; });
#else
            TimerRules rules = TimerRules.Default(settings.IsLongBreakAllowed, delegate { return settings.WorkSummaryEnabled; });
#endif
            TimerCoordinator coordinator = new TimerCoordinator(new SystemClock(), rules, new CsvWorkHistoryStore());
            System.Windows.Forms.Application.Run(new TrayApplicationContext(coordinator, settingsStore, new StartupRegistration(), settings));
        }
    }
}
