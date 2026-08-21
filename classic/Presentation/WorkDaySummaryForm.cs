using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class WorkDaySummaryForm : Form
    {
        internal WorkDaySummaryForm(WorkDaySummary summary)
        {
            Text = LocalizedText.WorkDaySummaryTitle;
            ClientSize = new Size(620, 325);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            Label range = new Label
            {
                Left = 20,
                Top = 74,
                Width = 580,
                Height = 25,
                Text = summary.HasEntries ? String.Format(LocalizedText.WorkDayRangeFormat, summary.StartedAt, summary.FinishedAt) : LocalizedText.NoWorkDayActivity,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            WorkDayTimelineControl timeline = new WorkDayTimelineControl(summary) { Left = 20, Top = 15, Width = 580 };
            TableLayoutPanel table = CreateSummaryTable(summary);
            Button close = new Button { Left = 500, Top = 280, Width = 100, Height = 30, Text = LocalizedText.Close, DialogResult = DialogResult.OK };

            Controls.AddRange(new Control[] { range, timeline, table, close });
            AcceptButton = close;
            CancelButton = close;
        }

        private static TableLayoutPanel CreateSummaryTable(WorkDaySummary summary)
        {
            TableLayoutPanel table = new TableLayoutPanel { Left = 20, Top = 108, Width = 580, Height = 156, ColumnCount = 2, RowCount = 6 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            for (int row = 0; row < 6; row++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            AddRow(table, 0, LocalizedText.TotalWork, summary.TotalWorkDuration, true, false);
            AddRow(table, 1, LocalizedText.Work, summary.WorkDuration, false, true);
            AddRow(table, 2, LocalizedText.Meetings, summary.MeetingDuration, false, true);
            AddRow(table, 3, LocalizedText.TotalRest, summary.BreakDuration, true, false);
            AddRow(table, 4, LocalizedText.ShortBreaks, summary.ShortBreakDuration, false, true);
            AddRow(table, 5, LocalizedText.LongBreaks, summary.LongBreakDuration, false, true);
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
