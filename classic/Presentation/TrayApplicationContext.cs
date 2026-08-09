using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;
using TimeTracker.Classic.Infrastructure;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class TrayApplicationContext : ApplicationContext
    {
        private readonly TimerCoordinator _coordinator;
        private readonly ISettingsStore _settingsStore;
        private readonly StartupRegistration _startup;
        private readonly NotifyIcon _trayIcon;
        private readonly Timer _timer;
        private readonly BreakOverlayForm _overlay;
        private readonly AppSettings _settings;

        internal TrayApplicationContext(TimerCoordinator coordinator, ISettingsStore settingsStore, StartupRegistration startup)
        {
            _coordinator = coordinator;
            _settingsStore = settingsStore;
            _startup = startup;
            _settings = settingsStore.Load();
            _overlay = new BreakOverlayForm(coordinator);

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Начать работу", delegate { StartWork(); });
            menu.MenuItems.Add("Настройки", delegate { ShowSettings(); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Выход", delegate { Exit(); });
            _trayIcon = new NotifyIcon { Icon = SystemIcons.Application, Text = "Time Tracker", ContextMenu = menu, Visible = true };
            _trayIcon.DoubleClick += delegate { StartWork(); };

            _timer = new Timer { Interval = 250 };
            _timer.Tick += delegate { _coordinator.Tick(); };
            _timer.Start();
        }

        private void StartWork()
        {
            try { _coordinator.Start(); }
            catch (InvalidOperationException) { }
        }

        private void ShowSettings()
        {
            using (SettingsForm form = new SettingsForm(_settings))
            {
                if (form.ShowDialog() != DialogResult.OK) return;
                form.ApplyTo(_settings);
                _settingsStore.Save(_settings);
                _startup.SetEnabled(_settings.StartWithWindows);
                _overlay.ApplyCaptureSetting(_settings.HideOverlayFromCapture);
            }
        }

        private void Exit()
        {
            _timer.Stop();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _overlay.Dispose();
            ExitThread();
        }
    }
}
