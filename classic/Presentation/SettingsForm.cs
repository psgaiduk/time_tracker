using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class SettingsForm : Form
    {
        private readonly CheckBox _hideFromCapture;
        private readonly CheckBox _showOnAllVirtualDesktops;
        private readonly CheckBox _startWithWindows;
        private readonly CheckBox _longBreakEnabled;
        private readonly CheckBox[] _longBreakDays;
        private readonly CheckBox _workSummaryEnabled;
        private readonly TextBox _workSummaryUrl;
        private readonly IApplicationCatalog _applicationCatalog;
        private readonly CheckBox _automaticMeetingEnabled;
        private readonly ListBox _automaticMeetingApplications;

        internal SettingsForm(AppSettings settings, IApplicationCatalog applicationCatalog)
        {
            _applicationCatalog = applicationCatalog;
            Text = "Настройки Time Tracker";
            ClientSize = new Size(570, 535);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            _hideFromCapture = new CheckBox { Left = 20, Top = 20, Width = 350, Text = "Скрывать полосу при демонстрации экрана", Checked = settings.HideOverlayFromCapture };
            _showOnAllVirtualDesktops = new CheckBox { Left = 20, Top = 52, Width = 390, Text = "Показывать полосу на всех рабочих столах", Checked = settings.ShowOverlayOnAllVirtualDesktops };
            _startWithWindows = new CheckBox { Left = 20, Top = 84, Width = 350, Text = "Запускать вместе с Windows", Checked = settings.StartWithWindows };
            _longBreakEnabled = new CheckBox { Left = 20, Top = 120, Width = 350, Text = "Включить большой перерыв", Checked = settings.LongBreakEnabled };
            string[] dayNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            bool[] dayValues = { settings.Monday, settings.Tuesday, settings.Wednesday, settings.Thursday, settings.Friday, settings.Saturday, settings.Sunday };
            _longBreakDays = new CheckBox[7];
            for (int index = 0; index < _longBreakDays.Length; index++)
            {
                _longBreakDays[index] = new CheckBox { Left = 40 + index * 52, Top = 150, Width = 48, Text = dayNames[index], Checked = dayValues[index], Enabled = settings.LongBreakEnabled };
                Controls.Add(_longBreakDays[index]);
            }
            _longBreakEnabled.CheckedChanged += delegate
            {
                foreach (CheckBox day in _longBreakDays) day.Enabled = _longBreakEnabled.Checked;
            };
            _workSummaryEnabled = new CheckBox { Left = 20, Top = 190, Width = 350, Text = "Показывать шаг «Заполни итоги работы»", Checked = settings.WorkSummaryEnabled };
            Label urlLabel = new Label { Left = 40, Top = 222, Width = 80, Text = "Ссылка:" };
            _workSummaryUrl = new TextBox { Left = 120, Top = 218, Width = 290, Text = settings.WorkSummaryUrl ?? String.Empty, Enabled = settings.WorkSummaryEnabled };
            _workSummaryEnabled.CheckedChanged += delegate { _workSummaryUrl.Enabled = _workSummaryEnabled.Checked; };
            _automaticMeetingEnabled = new CheckBox { Left = 20, Top = 260, Width = 500, Text = LocalizedText.AutomaticMeetingEnabled, Checked = settings.AutomaticMeetingEnabled };
            _automaticMeetingApplications = new ListBox { Left = 40, Top = 290, Width = 510, Height = 120 };
            foreach (string path in settings.AutomaticMeetingApplications) _automaticMeetingApplications.Items.Add(path);
            Button addRunning = new Button { Left = 40, Top = 420, Width = 160, Text = LocalizedText.AddRunningApplication };
            Button addExecutable = new Button { Left = 210, Top = 420, Width = 150, Text = LocalizedText.ChooseExecutable };
            Button remove = new Button { Left = 370, Top = 420, Width = 100, Text = LocalizedText.Remove };
            addRunning.Click += delegate { AddRunningApplication(); };
            addExecutable.Click += delegate { AddExecutable(); };
            remove.Click += delegate { if (_automaticMeetingApplications.SelectedIndex >= 0) _automaticMeetingApplications.Items.RemoveAt(_automaticMeetingApplications.SelectedIndex); };
            Button save = new Button { Left = 380, Top = 485, Width = 85, Text = "Сохранить", DialogResult = DialogResult.OK };
            Button cancel = new Button { Left = 475, Top = 485, Width = 75, Text = "Отмена", DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { _hideFromCapture, _showOnAllVirtualDesktops, _startWithWindows, _longBreakEnabled, _workSummaryEnabled, urlLabel, _workSummaryUrl, _automaticMeetingEnabled, _automaticMeetingApplications, addRunning, addExecutable, remove, save, cancel });
            AcceptButton = save;
            CancelButton = cancel;
        }

        internal void ApplyTo(AppSettings settings)
        {
            settings.HideOverlayFromCapture = _hideFromCapture.Checked;
            settings.ShowOverlayOnAllVirtualDesktops = _showOnAllVirtualDesktops.Checked;
            settings.StartWithWindows = _startWithWindows.Checked;
            settings.LongBreakEnabled = _longBreakEnabled.Checked;
            settings.Monday = _longBreakDays[0].Checked;
            settings.Tuesday = _longBreakDays[1].Checked;
            settings.Wednesday = _longBreakDays[2].Checked;
            settings.Thursday = _longBreakDays[3].Checked;
            settings.Friday = _longBreakDays[4].Checked;
            settings.Saturday = _longBreakDays[5].Checked;
            settings.Sunday = _longBreakDays[6].Checked;
            settings.WorkSummaryEnabled = _workSummaryEnabled.Checked;
            settings.WorkSummaryUrl = _workSummaryUrl.Text.Trim();
            settings.AutomaticMeetingEnabled = _automaticMeetingEnabled.Checked;
            settings.AutomaticMeetingApplications.Clear();
            foreach (object item in _automaticMeetingApplications.Items) settings.AutomaticMeetingApplications.Add(item.ToString());
        }

        private void AddRunningApplication()
        {
            using (RunningApplicationForm form = new RunningApplicationForm(_applicationCatalog))
                if (form.ShowDialog(this) == DialogResult.OK && form.SelectedApplication != null) AddPath(form.SelectedApplication.ExecutablePath);
        }

        private void AddExecutable()
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Filter = "Applications (*.exe)|*.exe", CheckFileExists = true })
                if (dialog.ShowDialog(this) == DialogResult.OK) AddPath(dialog.FileName);
        }

        private void AddPath(string path)
        {
            foreach (object item in _automaticMeetingApplications.Items)
                if (String.Equals(item.ToString(), path, StringComparison.OrdinalIgnoreCase)) return;
            _automaticMeetingApplications.Items.Add(path);
        }
    }
}
