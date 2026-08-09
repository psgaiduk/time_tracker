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
#if TEST_TIMER
            TimerRules rules = TimerRules.Test();
#else
            TimerRules rules = TimerRules.Default();
#endif
            TimerCoordinator coordinator = new TimerCoordinator(new SystemClock(), rules);
            System.Windows.Forms.Application.Run(new TrayApplicationContext(coordinator, new PortableSettingsStore(), new StartupRegistration()));
        }
    }
}
