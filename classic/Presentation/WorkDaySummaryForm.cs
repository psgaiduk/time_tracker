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
            ClientSize = new Size(620, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            Label range = CreateLabel(20, 18, 580, 25, summary.HasEntries ?
                String.Format(LocalizedText.WorkDayRangeFormat, summary.StartedAt, summary.FinishedAt) : LocalizedText.NoWorkDayActivity);
            range.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            Label total = CreateLabel(20, 52, 580, 22, String.Format(LocalizedText.WorkDayTotalFormat, Format(summary.TotalDuration)));
            Label totalWork = CreateLabel(20, 78, 280, 22, String.Format(LocalizedText.WorkDayTotalWorkFormat, Format(summary.TotalWorkDuration)));
            Label rest = CreateLabel(320, 78, 280, 22, String.Format(LocalizedText.WorkDayRestFormat, Format(summary.BreakDuration)));
            Label work = CreateLabel(20, 104, 280, 22, String.Format(LocalizedText.WorkDayWorkFormat, Format(summary.WorkDuration)));
            Label meetings = CreateLabel(320, 104, 280, 22, String.Format(LocalizedText.WorkDayMeetingFormat, Format(summary.MeetingDuration)));

            WorkDayTimelineControl timeline = new WorkDayTimelineControl(summary) { Left = 20, Top = 138, Width = 580 };
            Label legend = CreateLabel(20, 200, 580, 42, LocalizedText.WorkDayLegend);
            Button close = new Button { Left = 500, Top = 250, Width = 100, Height = 30, Text = LocalizedText.Close, DialogResult = DialogResult.OK };
            Controls.AddRange(new Control[] { range, total, totalWork, work, meetings, rest, timeline, legend, close });
            AcceptButton = close;
            CancelButton = close;
        }

        private static Label CreateLabel(int left, int top, int width, int height, string text)
        {
            return new Label { Left = left, Top = top, Width = width, Height = height, Text = text };
        }

        private static string Format(TimeSpan value)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", (int)value.TotalHours, value.Minutes, value.Seconds);
        }
    }
}
