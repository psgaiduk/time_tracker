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
        private readonly Label _message;
        private readonly Label _remaining;
        private readonly Button _skip;
        private readonly Button _rest;
        private bool _excludeFromCapture;

        internal BreakOverlayForm(TimerCoordinator coordinator)
        {
            _coordinator = coordinator;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Bounds = new Rectangle(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top, Screen.PrimaryScreen.WorkingArea.Width, 64);

            _message = new Label { Left = 15, Top = 0, Width = 300, Height = 64, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White, Font = new Font("Segoe UI", 13, FontStyle.Bold) };
            _remaining = new Label { Left = 320, Top = 0, Width = 100, Height = 64, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Consolas", 18, FontStyle.Bold) };
            _skip = new Button { Width = 100, Height = 36, Text = "Пропустить", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _rest = new Button { Width = 100, Height = 36, Text = "Отдыхать", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _skip.Location = new Point(ClientSize.Width - 220, 14);
            _rest.Location = new Point(ClientSize.Width - 110, 14);
            _skip.Click += delegate { _coordinator.Skip(); };
            _rest.Click += delegate { _coordinator.Rest(); };
            Controls.AddRange(new Control[] { _message, _remaining, _skip, _rest });
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
            _remaining.Text = String.Format("{0:00}:{1:00}", (int)state.Remaining.TotalMinutes, state.Remaining.Seconds);
            _skip.Visible = state.Phase == TimerPhase.AwaitingBreakDecision;
            _rest.Visible = state.Phase == TimerPhase.AwaitingBreakDecision;
            if (state.Phase == TimerPhase.AwaitingBreakDecision)
            {
                BackColor = Color.Firebrick;
                _message.Text = "Рабочий интервал завершён";
                Show();
            }
            else if (state.Phase == TimerPhase.ShortBreak || state.Phase == TimerPhase.LongBreak)
            {
                BackColor = Color.SeaGreen;
                _message.Text = state.Phase == TimerPhase.LongBreak ? "Большой перерыв" : "Короткий перерыв";
                Show();
            }
            else
            {
                BackColor = Color.FromArgb(51, 65, 85);
                _message.Text = "Работа";
                Hide();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr window, uint affinity);
    }
}
