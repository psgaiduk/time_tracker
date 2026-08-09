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
        private readonly CheckBox _longBreakEnabled;
        private readonly CheckBox[] _longBreakDays;

        internal SettingsForm(AppSettings settings)
        {
            Text = "Настройки Time Tracker";
            ClientSize = new Size(430, 215);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            _hideFromCapture = new CheckBox { Left = 20, Top = 20, Width = 350, Text = "Скрывать полосу при демонстрации экрана", Checked = settings.HideOverlayFromCapture };
            _startWithWindows = new CheckBox { Left = 20, Top = 52, Width = 350, Text = "Запускать вместе с Windows", Checked = settings.StartWithWindows };
            _longBreakEnabled = new CheckBox { Left = 20, Top = 88, Width = 350, Text = "Включить большой перерыв", Checked = settings.LongBreakEnabled };
            string[] dayNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            bool[] dayValues = { settings.Monday, settings.Tuesday, settings.Wednesday, settings.Thursday, settings.Friday, settings.Saturday, settings.Sunday };
            _longBreakDays = new CheckBox[7];
            for (int index = 0; index < _longBreakDays.Length; index++)
            {
                _longBreakDays[index] = new CheckBox { Left = 40 + index * 52, Top = 118, Width = 48, Text = dayNames[index], Checked = dayValues[index], Enabled = settings.LongBreakEnabled };
                Controls.Add(_longBreakDays[index]);
            }
            _longBreakEnabled.CheckedChanged += delegate
            {
                foreach (CheckBox day in _longBreakDays) day.Enabled = _longBreakEnabled.Checked;
            };
            Button save = new Button { Left = 245, Top = 165, Width = 85, Text = "Сохранить", DialogResult = DialogResult.OK };
            Button cancel = new Button { Left = 340, Top = 165, Width = 75, Text = "Отмена", DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { _hideFromCapture, _startWithWindows, _longBreakEnabled, save, cancel });
            AcceptButton = save;
            CancelButton = cancel;
        }

        internal void ApplyTo(AppSettings settings)
        {
            settings.HideOverlayFromCapture = _hideFromCapture.Checked;
            settings.StartWithWindows = _startWithWindows.Checked;
            settings.LongBreakEnabled = _longBreakEnabled.Checked;
            settings.Monday = _longBreakDays[0].Checked;
            settings.Tuesday = _longBreakDays[1].Checked;
            settings.Wednesday = _longBreakDays[2].Checked;
            settings.Thursday = _longBreakDays[3].Checked;
            settings.Friday = _longBreakDays[4].Checked;
            settings.Saturday = _longBreakDays[5].Checked;
            settings.Sunday = _longBreakDays[6].Checked;
        }
    }
}
