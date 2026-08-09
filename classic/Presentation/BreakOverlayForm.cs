using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TimeTracker.Classic.Application;
using TimeTracker.Classic.Domain;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class BreakOverlayForm : Form
    {
        private const uint WdaExcludeFromCapture = 0x11;
        private readonly TimerCoordinator _coordinator;
        private readonly AppSettings _settings;
        private readonly Label _message;
        private readonly Label _remaining;
        private readonly Label _continuous;
        private readonly Label _today;
        private readonly Button _skip;
        private readonly Button _rest;
        private readonly Button _endBreak;
        private readonly Button _continueToBreak;
        private readonly LinkLabel _summaryLink;
        private bool _excludeFromCapture;

        internal BreakOverlayForm(TimerCoordinator coordinator, AppSettings settings)
        {
            _coordinator = coordinator;
            _settings = settings;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Bounds = new Rectangle(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top, Screen.PrimaryScreen.WorkingArea.Width, 64);

            _message = new Label { Left = 15, Top = 0, Width = 250, Height = 64, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            _remaining = new Label { Left = 265, Top = 0, Width = 165, Height = 64, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Consolas", 17, FontStyle.Bold) };
            _continuous = new Label { Left = 440, Top = 0, Width = 175, Height = 64, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White };
            _today = new Label { Left = 620, Top = 0, Width = 175, Height = 64, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White };
            _skip = new Button { Width = 100, Height = 36, Text = "Пропустить", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _rest = new Button { Width = 100, Height = 36, Text = "Отдыхать", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _endBreak = new Button { Width = 150, Height = 36, Text = "Завершить отдых", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _continueToBreak = new Button { Width = 160, Height = 36, Text = "Перейти к отдыху", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _summaryLink = new LinkLabel { Left = 205, Top = 0, Width = 590, Height = 64, TextAlign = ContentAlignment.MiddleLeft, LinkColor = Color.White, ActiveLinkColor = Color.Yellow, BackColor = Color.Transparent };
            _skip.Location = new Point(ClientSize.Width - 220, 14);
            _rest.Location = new Point(ClientSize.Width - 110, 14);
            _endBreak.Location = new Point(ClientSize.Width - 160, 14);
            _continueToBreak.Location = new Point(ClientSize.Width - 170, 14);
            _skip.Click += delegate { _coordinator.Skip(); };
            _rest.Click += delegate { _coordinator.Rest(); };
            _endBreak.Click += delegate { _coordinator.EndBreak(); };
            _continueToBreak.Click += delegate { _coordinator.CompleteWorkSummary(); };
            _summaryLink.LinkClicked += delegate
            {
                try { System.Diagnostics.Process.Start(_settings.WorkSummaryUrl); }
                catch (Exception error) { MessageBox.Show("Не удалось открыть ссылку:\n" + error.Message, "Time Tracker"); }
            };
            Controls.AddRange(new Control[] { _message, _remaining, _continuous, _today, _summaryLink, _skip, _rest, _endBreak, _continueToBreak });
            _coordinator.StateChanged += delegate { UpdateState(); };
            UpdateState();
        }

        internal void ApplyCaptureSetting(bool enabled)
        {
            _excludeFromCapture = enabled;
            ApplyDisplayAffinity();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyDisplayAffinity();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyDisplayAffinity();
        }

        private void ApplyDisplayAffinity()
        {
            if (!IsHandleCreated) return;
            SetWindowDisplayAffinity(Handle, _excludeFromCapture ? WdaExcludeFromCapture : 0);
        }

        private void UpdateState()
        {
            TimerState state = _coordinator.State;
            DailyWorkStats stats = _coordinator.Stats;
            _remaining.Text = state.Phase == TimerPhase.AwaitingBreakDecision ? Format(stats.CurrentPeriod) : Format(state.Remaining);
            _continuous.Text = "Без отдыха: " + Format(stats.ContinuousWork);
            _today.Text = "Сегодня: " + Format(stats.WorkedToday);
            _skip.Visible = state.Phase == TimerPhase.AwaitingBreakDecision;
            _rest.Visible = state.Phase == TimerPhase.AwaitingBreakDecision;
            _endBreak.Visible = state.Phase == TimerPhase.ShortBreak || state.Phase == TimerPhase.LongBreak;
            _continueToBreak.Visible = state.Phase == TimerPhase.WorkSummary;
            _summaryLink.Visible = state.Phase == TimerPhase.WorkSummary && !String.IsNullOrWhiteSpace(_settings.WorkSummaryUrl);
            if (state.Phase == TimerPhase.WorkSummary)
            {
                BackColor = Color.RoyalBlue;
                _message.Text = _summaryLink.Visible ? "Заполни итоги работы:" : "Заполни итоги работы";
                _message.Width = _summaryLink.Visible ? 190 : 250;
                _summaryLink.Text = _settings.WorkSummaryUrl;
                _remaining.Visible = false;
                _continuous.Visible = false;
                _today.Visible = false;
                Show();
            }
            else if (state.Phase == TimerPhase.AwaitingBreakDecision)
            {
                RestoreStatisticsVisibility();
                BackColor = Color.Firebrick;
                _message.Text = "Рабочий интервал завершён";
                Show();
            }
            else if (state.Phase == TimerPhase.ShortBreak || state.Phase == TimerPhase.LongBreak)
            {
                RestoreStatisticsVisibility();
                BackColor = Color.SeaGreen;
                _message.Text = state.Phase == TimerPhase.LongBreak ? "Большой перерыв" : "Короткий перерыв";
                Show();
            }
            else
            {
                RestoreStatisticsVisibility();
                BackColor = Color.FromArgb(51, 65, 85);
                _message.Text = "Работа";
                Hide();
            }
        }

        private void RestoreStatisticsVisibility()
        {
            _message.Height = 64;
            _message.Width = 250;
            _remaining.Visible = true;
            _continuous.Visible = true;
            _today.Visible = true;
        }

        private static string Format(TimeSpan value)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", (int)value.TotalHours, value.Minutes, value.Seconds);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr window, uint affinity);
    }
}
