using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class SettingsForm : Form
    {
        private readonly CheckBox _hideFromCapture;
        private readonly CheckBox _startWithWindows;

        internal SettingsForm(AppSettings settings)
        {
            Text = "Настройки Time Tracker";
            ClientSize = new Size(390, 150);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            _hideFromCapture = new CheckBox { Left = 20, Top = 20, Width = 350, Text = "Скрывать полосу при демонстрации экрана", Checked = settings.HideOverlayFromCapture };
            _startWithWindows = new CheckBox { Left = 20, Top = 52, Width = 350, Text = "Запускать вместе с Windows", Checked = settings.StartWithWindows };
            Button save = new Button { Left = 205, Top = 98, Width = 80, Text = "Сохранить", DialogResult = DialogResult.OK };
            Button cancel = new Button { Left = 295, Top = 98, Width = 75, Text = "Отмена", DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { _hideFromCapture, _startWithWindows, save, cancel });
            AcceptButton = save;
            CancelButton = cancel;
        }

        internal void ApplyTo(AppSettings settings)
        {
            settings.HideOverlayFromCapture = _hideFromCapture.Checked;
            settings.StartWithWindows = _startWithWindows.Checked;
        }
    }
}
