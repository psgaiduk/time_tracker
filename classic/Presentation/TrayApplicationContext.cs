using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;
using TimeTracker.Classic.Domain;
using TimeTracker.Classic.Infrastructure;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class TrayApplicationContext : ApplicationContext
    {
        private readonly TimerCoordinator _coordinator;
        private readonly ISettingsStore _settingsStore;
        private readonly StartupRegistration _startup;
        private readonly NotifyIcon _trayIcon;
        private readonly MenuItem _statusItem;
        private readonly MenuItem _statsItem;
        private readonly Timer _timer;
        private readonly BreakOverlayForm _overlay;
        private readonly AppSettings _settings;
        private Icon _dynamicIcon;
        private string _iconKey;

        internal TrayApplicationContext(TimerCoordinator coordinator, TimerRules rules, ISettingsStore settingsStore, StartupRegistration startup, AppSettings settings, Action<IntPtr, bool> setVirtualDesktopPinning, Action playBreakCompletedSound, Action<bool> setActivitySimulationEnabled)
        {
            _coordinator = coordinator;
            _settingsStore = settingsStore;
            _startup = startup;
            _settings = settings;
            _overlay = new BreakOverlayForm(coordinator, rules, settings, setVirtualDesktopPinning, playBreakCompletedSound, setActivitySimulationEnabled);
            _overlay.ApplyCaptureSetting(_settings.HideOverlayFromCapture);
            _overlay.ApplyVirtualDesktopSetting(_settings.ShowOverlayOnAllVirtualDesktops);

            ContextMenu menu = new ContextMenu();
            _statusItem = new MenuItem("Начать работу", delegate { StartWork(); });
            menu.MenuItems.Add(_statusItem);
            _statsItem = new MenuItem("Сегодня: 00:00:00") { Enabled = false };
            menu.MenuItems.Add(_statsItem);
            menu.MenuItems.Add("Настройки", delegate { ShowSettings(); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Выход", delegate { Exit(); });
            _trayIcon = new NotifyIcon { Icon = SystemIcons.Application, Text = "Time Tracker", ContextMenu = menu, Visible = true };
            _trayIcon.DoubleClick += delegate { HandleTrayDoubleClick(); };
            _coordinator.StateChanged += delegate { UpdateTrayStatus(); };

            _timer = new Timer { Interval = 250 };
            _timer.Tick += delegate { _coordinator.Tick(); };
            _timer.Start();
            UpdateTrayStatus();
        }

        private void StartWork()
        {
            try { _coordinator.Start(); }
            catch (InvalidOperationException) { }
        }

        private void HandleTrayDoubleClick()
        {
            if (_coordinator.State.Phase == TimerPhase.Work)
            {
                _coordinator.StartShortBreak();
                return;
            }
            StartWork();
        }

        private void UpdateTrayStatus()
        {
            string status = TrayStatusText.Format(_coordinator.State);
            _statusItem.Text = status;
            _statusItem.Enabled = _coordinator.State.Phase == TimeTracker.Classic.Domain.TimerPhase.Idle;
            _statsItem.Text = "Без отдыха: " + Format(_coordinator.Stats.ContinuousWork) + " | Сегодня: " + Format(_coordinator.Stats.WorkedToday);
            _trayIcon.Text = "Time Tracker: " + status;
            UpdateTrayIcon();
        }

        private void UpdateTrayIcon()
        {
            string key = TrayIconRenderer.GetKey(_coordinator.State, _coordinator.Stats);
            if (key == _iconKey) return;
            Icon next = TrayIconRenderer.Create(_coordinator.State, _coordinator.Stats);
            _trayIcon.Icon = next;
            if (_dynamicIcon != null) _dynamicIcon.Dispose();
            _dynamicIcon = next;
            _iconKey = key;
        }

        private static string Format(TimeSpan value)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", (int)value.TotalHours, value.Minutes, value.Seconds);
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
                _overlay.ApplyVirtualDesktopSetting(_settings.ShowOverlayOnAllVirtualDesktops);
            }
        }

        private void Exit()
        {
            _timer.Stop();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            if (_dynamicIcon != null) _dynamicIcon.Dispose();
            _overlay.Dispose();
            ExitThread();
        }
    }
}
