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
            IClock clock = new SystemClock();
#if TEST_TIMER
            TimerRules rules = TimerRules.Test(delegate(DateTime date) { return true; }, delegate { return settings.WorkSummaryEnabled; });
            UserInactivityBreakTrigger userInactivityTrigger = UserInactivityBreakTrigger.CreateTest(clock);
#else
            TimerRules rules = TimerRules.Default(delegate(DateTime date) { return true; }, delegate { return settings.WorkSummaryEnabled; });
            UserInactivityBreakTrigger userInactivityTrigger = UserInactivityBreakTrigger.CreateDefault(clock);
#endif
            TimerCoordinator coordinator = new TimerCoordinator(clock, rules, new CsvWorkHistoryStore());
            IUserInactivity userInactivity = new WindowsUserInactivity();
            VirtualDesktopWindowPinning windowPinning = new VirtualDesktopWindowPinning();
            WindowsNotificationSound notificationSound = new WindowsNotificationSound();
            WindowsForegroundApplication foregroundApplication = new WindowsForegroundApplication();
            WindowsApplicationCatalog applicationCatalog = new WindowsApplicationCatalog();
            using (WindowsActivitySimulator activitySimulator = new WindowsActivitySimulator())
                System.Windows.Forms.Application.Run(new TrayApplicationContext(coordinator, rules, settingsStore, new StartupRegistration(), settings, foregroundApplication, applicationCatalog, userInactivityTrigger, userInactivity, windowPinning.SetPinned, notificationSound.PlayBreakCompleted, activitySimulator.SetEnabled));
        }
    }
}
