using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class WorkDaySummaryForm : Form
    {
        private readonly Func<DateTime, WorkDaySummary> _loadSummary;
        private readonly DateTime _latestDate;
        private readonly Label _selectedDateLabel;
        private readonly Label _range;
        private readonly Button _previousDay;
        private readonly Button _nextDay;
        private readonly Button _copy;
        private DateTime _selectedDate;
        private WorkDaySummary _summary;
        private WorkDayTimelineControl _timeline;
        private TableLayoutPanel _table;

        internal WorkDaySummaryForm(WorkDaySummary summary) : this(summary, null) { }

        internal WorkDaySummaryForm(WorkDaySummary summary, Func<DateTime, WorkDaySummary> loadSummary)
        {
            _summary = summary;
            _selectedDate = summary.FinishedAt.Date;
            _latestDate = _selectedDate;
            _loadSummary = loadSummary;
            Text = LocalizedText.WorkDaySummaryTitle;
            ClientSize = new Size(620, 310);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            _selectedDateLabel = new Label
            {
                Name = "SelectedDate",
                Left = 260,
                Top = 14,
                Width = 100,
                Height = 28,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _range = new Label
            {
                Left = 20,
                Top = 109,
                Width = 580,
                Height = 25,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            _previousDay = new Button { Name = "PreviousDay", Left = 220, Top = 14, Width = 28, Height = 28, Text = "←", AccessibleName = LocalizedText.PreviousDay, Enabled = loadSummary != null };
            _nextDay = new Button { Name = "NextDay", Left = 372, Top = 14, Width = 28, Height = 28, Text = "→", AccessibleName = LocalizedText.NextDay, Enabled = false };
            _previousDay.Click += delegate { ShowPreviousDay(); };
            _nextDay.Click += delegate { ShowNextDay(); };
            _copy = new Button { Left = 380, Top = 265, Width = 110, Height = 30, Text = LocalizedText.Copy };
            _copy.Click += delegate { CopyReport(); };
            Button close = new Button { Left = 500, Top = 265, Width = 100, Height = 30, Text = LocalizedText.Close, DialogResult = DialogResult.OK };

            Controls.AddRange(new Control[] { _selectedDateLabel, _range, _previousDay, _nextDay, _copy, close });
            ShowSummary(summary);
            AcceptButton = close;
            CancelButton = close;
        }

        private void ShowDay(DateTime day)
        {
            if (_loadSummary == null || day.Date > _latestDate) return;
            _selectedDate = day.Date;
            ShowSummary(_loadSummary(_selectedDate));
        }

        internal void ShowPreviousDay()
        {
            ShowDay(_selectedDate.AddDays(-1));
        }

        internal void ShowNextDay()
        {
            ShowDay(_selectedDate.AddDays(1));
        }

        private void ShowSummary(WorkDaySummary summary)
        {
            _summary = summary;
            if (_timeline != null) { Controls.Remove(_timeline); _timeline.Dispose(); }
            if (_table != null) { Controls.Remove(_table); _table.Dispose(); }
            _selectedDateLabel.Text = _selectedDate.ToString("dd.MM.yyyy");
            _range.Text = summary.HasEntries ? String.Format(LocalizedText.WorkDayRangeFormat, summary.StartedAt, summary.FinishedAt) : String.Format(LocalizedText.NoWorkDayActivityForDateFormat, _selectedDate);
            _timeline = new WorkDayTimelineControl(summary) { Left = 20, Top = 50, Width = 580 };
            _table = CreateSummaryTable(summary);
            Controls.Add(_timeline);
            Controls.Add(_table);
            _timeline.SendToBack();
            _copy.Enabled = summary.HasEntries;
            _previousDay.Enabled = _loadSummary != null;
            _nextDay.Enabled = _loadSummary != null && _selectedDate < _latestDate;
        }

        private void CopyReport()
        {
            try { Clipboard.SetText(GetReportText()); }
            catch (Exception) { MessageBox.Show(this, LocalizedText.CopyFailed, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        internal string GetReportText()
        {
            return WorkDayReportText.Format(_summary);
        }

        private static TableLayoutPanel CreateSummaryTable(WorkDaySummary summary)
        {
            TableLayoutPanel table = new TableLayoutPanel { Left = 20, Top = 143, Width = 580, Height = 104, ColumnCount = 2, RowCount = 4 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            for (int row = 0; row < 4; row++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            AddRow(table, 0, LocalizedText.TotalWork, summary.TotalWorkDuration, true, false);
            AddRow(table, 1, LocalizedText.Work, summary.WorkDuration, false, true);
            AddRow(table, 2, LocalizedText.Meetings, summary.MeetingDuration, false, true);
            AddRow(table, 3, LocalizedText.TotalRest, summary.BreakDuration, true, false);
            return table;
        }

        private static void AddRow(TableLayoutPanel table, int row, string name, TimeSpan duration, bool bold, bool indented)
        {
            Label nameLabel = new Label { Dock = DockStyle.Fill, Text = (indented ? "  — " : String.Empty) + name, TextAlign = ContentAlignment.MiddleLeft };
            Label valueLabel = new Label { Dock = DockStyle.Fill, Text = Format(duration), TextAlign = ContentAlignment.MiddleRight };
            if (bold)
            {
                nameLabel.Font = new Font(nameLabel.Font, FontStyle.Bold);
                valueLabel.Font = new Font(valueLabel.Font, FontStyle.Bold);
            }
            table.Controls.Add(nameLabel, 0, row);
            table.Controls.Add(valueLabel, 1, row);
        }

        private static string Format(TimeSpan value)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", (int)value.TotalHours, value.Minutes, value.Seconds);
        }
    }
}
