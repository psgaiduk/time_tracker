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
        private readonly IClock _clock;
        private readonly ISettingsStore _settingsStore;
        private readonly StartupRegistration _startup;
        private readonly NotifyIcon _trayIcon;
        private readonly MenuItem _statusItem;
        private readonly MenuItem _statsItem;
        private readonly Timer _timer;
        private readonly Timer _singleClickTimer;
        private readonly Timer _foregroundTimer;
        private readonly BreakOverlayForm _overlay;
        private readonly AppSettings _settings;
        private readonly IForegroundApplication _foregroundApplication;
        private readonly IApplicationCatalog _applicationCatalog;
        private readonly UserInactivityBreakTrigger _userInactivityTrigger;
        private readonly IUserInactivity _userInactivity;
        private readonly UserInactivityShutdownTrigger _shutdownTrigger;
        private readonly UserActivityStartTrigger _activityStartTrigger;
        private readonly HistoryApiClient _historyApiClient;
        private DateTime _lastScheduledStartUploadDate;
        private DateTime _lastScheduledEndUploadDate;
        private Icon _dynamicIcon;
        private string _iconKey;

        internal TrayApplicationContext(TimerCoordinator coordinator, IClock clock, TimerRules rules, ISettingsStore settingsStore, StartupRegistration startup, AppSettings settings, IForegroundApplication foregroundApplication, IApplicationCatalog applicationCatalog, UserInactivityBreakTrigger userInactivityTrigger, IUserInactivity userInactivity, UserInactivityShutdownTrigger shutdownTrigger, UserActivityStartTrigger activityStartTrigger, Action<IntPtr, bool> setVirtualDesktopPinning, Action playBreakCompletedSound, Action<bool> setActivitySimulationEnabled)
        {
            _coordinator = coordinator;
            _clock = clock;
            _settingsStore = settingsStore;
            _startup = startup;
            _settings = settings;
            _foregroundApplication = foregroundApplication;
            _applicationCatalog = applicationCatalog;
            _userInactivityTrigger = userInactivityTrigger;
            _userInactivity = userInactivity;
            _shutdownTrigger = shutdownTrigger;
            _activityStartTrigger = activityStartTrigger;
            _historyApiClient = new HistoryApiClient();
            _overlay = new BreakOverlayForm(coordinator, rules, settings, setVirtualDesktopPinning, playBreakCompletedSound, setActivitySimulationEnabled);
            _overlay.ApplyCaptureSetting(_settings.HideOverlayFromCapture);
            _overlay.ApplyVirtualDesktopSetting(_settings.ShowOverlayOnAllVirtualDesktops);

            ContextMenu menu = new ContextMenu();
            _statusItem = new MenuItem("Начать работу", delegate { StartWork(); });
            menu.MenuItems.Add(_statusItem);
            _statsItem = new MenuItem("Сегодня: 00:00:00") { Enabled = false };
            menu.MenuItems.Add(_statsItem);
            menu.MenuItems.Add(LocalizedText.FinishWorkDay, delegate { FinishWorkDay(); });
            menu.MenuItems.Add("Настройки", delegate { ShowSettings(); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Выход", delegate { Exit(); });
            _trayIcon = new NotifyIcon { Icon = SystemIcons.Application, Text = "Time Tracker", ContextMenu = menu, Visible = true };
            _singleClickTimer = new Timer { Interval = SystemInformation.DoubleClickTime };
            _singleClickTimer.Tick += delegate { _singleClickTimer.Stop(); HandleTraySingleClick(); };
            _trayIcon.MouseClick += delegate(object sender, MouseEventArgs args)
            {
                if (args.Button != MouseButtons.Left) return;
                _singleClickTimer.Stop();
                _singleClickTimer.Start();
            };
            _trayIcon.DoubleClick += delegate { _singleClickTimer.Stop(); HandleTrayDoubleClick(); };
            _coordinator.StateChanged += delegate { HandleStateChanged(); };

            _timer = new Timer { Interval = 250 };
            _timer.Tick += delegate { _coordinator.Tick(); };
            _timer.Start();
            _foregroundTimer = new Timer { Interval = 1000 };
            _foregroundTimer.Tick += delegate
            {
                _coordinator.UpdateAutomaticMeeting(_settings.IsAutomaticMeetingApplication(_foregroundApplication.GetExecutablePath()));
                UpdateUserInactivity();
                UploadScheduledHistory(_clock.Now);
            };
            _foregroundTimer.Start();
            _lastPhase = _coordinator.State.Phase;
            UpdateTrayStatus();
        }

        private TimerPhase _lastPhase;

        private void HandleStateChanged()
        {
            TimerPhase phase = _coordinator.State.Phase;
            if (_lastPhase == TimerPhase.Idle && phase == TimerPhase.Work)
                UploadHistory(_clock.Now.Date);
            else if (IsBreak(phase) || phase == TimerPhase.Idle && _lastPhase != TimerPhase.Idle)
                UploadHistory(_clock.Now.Date);
            _lastPhase = phase;
            UpdateTrayStatus();
        }

        private void UploadHistory(DateTime day)
        {
            DateTime now = _clock.Now;
            if (String.IsNullOrWhiteSpace(_settings.HistoryApiUrl) || String.IsNullOrWhiteSpace(_settings.UserId) || String.IsNullOrWhiteSpace(_settings.HistoryApiToken)) return;
            _historyApiClient.UploadAsync(_settings.HistoryApiUrl, _settings.UserId, _settings.HistoryApiToken, day, _coordinator.GetHistory(day));
        }

        private void UploadScheduledHistory(DateTime now)
        {
            if (now.TimeOfDay >= _settings.WorkDayStart && _lastScheduledStartUploadDate != now.Date)
                UploadStartOfWorkDay(now);
            if (now.TimeOfDay >= _settings.WorkDayEnd && _lastScheduledEndUploadDate != now.Date)
            {
                _lastScheduledEndUploadDate = now.Date;
                UploadHistory(now.Date);
            }
        }

        private void UploadStartOfWorkDay(DateTime now)
        {
            if (_lastScheduledStartUploadDate == now.Date) return;
            _lastScheduledStartUploadDate = now.Date;
            UploadHistory(now.Date.AddDays(-1));
            UploadHistory(now.Date);
        }

        private void StartWork()
        {
            try { _coordinator.Start(); }
            catch (InvalidOperationException) { }
        }

        private void UpdateUserInactivity()
        {
            try
            {
                DateTime now = _clock.Now;
                TimeSpan inactiveFor = _userInactivity.GetInactiveDuration();
                TimerPhase phase = _coordinator.State.Phase;
                if (phase != TimerPhase.Idle && _shutdownTrigger.ShouldShutdown(now, inactiveFor, _settings.WorkDayStart, _settings.WorkDayEnd))
                {
                    _coordinator.Stop();
                    return;
                }
                if (phase == TimerPhase.Idle && _activityStartTrigger.ShouldStart(now, inactiveFor, _settings.WorkDayStart, _settings.WorkDayEnd))
                {
                    StartWork();
                    return;
                }
                if (_userInactivityTrigger.ShouldStartBreak(phase, inactiveFor))
                {
                    if (phase == TimerPhase.AwaitingBreakDecision) _coordinator.Rest();
                    else if (phase == TimerPhase.WorkSummary) _coordinator.CompleteWorkSummary();
                    else _coordinator.StartShortBreak();
                }
            }
            catch (Exception) { }
        }

        private void HandleTrayDoubleClick()
        {
            if (_coordinator.State.Phase == TimerPhase.Work || _coordinator.State.Phase == TimerPhase.Meeting)
            {
                _coordinator.StartShortBreak();
                return;
            }
            StartWork();
        }

        private void HandleTraySingleClick()
        {
            if (_coordinator.State.Phase != TimerPhase.Work && _coordinator.State.Phase != TimerPhase.Meeting) return;
            _coordinator.ToggleMeeting();
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
            using (SettingsForm form = new SettingsForm(_settings, _applicationCatalog))
            {
                if (form.ShowDialog() != DialogResult.OK) return;
                form.ApplyTo(_settings);
                _settingsStore.Save(_settings);
                _startup.SetEnabled(_settings.StartWithWindows);
                _overlay.ApplyCaptureSetting(_settings.HideOverlayFromCapture);
                _overlay.ApplyVirtualDesktopSetting(_settings.ShowOverlayOnAllVirtualDesktops);
            }
        }

        private void FinishWorkDay()
        {
            WorkDaySummary summary = _coordinator.FinishWorkDay();
            UploadHistory(_clock.Now.Date);
            using (WorkDaySummaryForm form = new WorkDaySummaryForm(summary, delegate(DateTime day) { return _coordinator.GetWorkDaySummary(day); }))
                form.ShowDialog();
        }

        private void Exit()
        {
            _timer.Stop();
            _foregroundTimer.Stop();
            _foregroundTimer.Dispose();
            _singleClickTimer.Stop();
            _singleClickTimer.Dispose();
            _coordinator.Stop();
            UploadHistory(_clock.Now.Date);
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            if (_dynamicIcon != null) _dynamicIcon.Dispose();
            _overlay.Dispose();
            ExitThread();
        }

        private static bool IsBreak(TimerPhase phase)
        {
            return phase == TimerPhase.ShortBreak || phase == TimerPhase.LongBreak;
        }
    }
}
