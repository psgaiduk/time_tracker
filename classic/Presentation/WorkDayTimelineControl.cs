using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class WorkDayTimelineControl : Control
    {
        private readonly WorkDaySummary _summary;

        internal WorkDayTimelineControl(WorkDaySummary summary)
        {
            _summary = summary;
            DoubleBuffered = true;
            Height = 54;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle bar = new Rectangle(0, 4, ClientSize.Width, 28);
            using (Brush background = new SolidBrush(Color.FromArgb(203, 213, 225)))
                e.Graphics.FillRectangle(background, bar);
            if (!_summary.HasEntries || _summary.TotalDuration <= TimeSpan.Zero) return;

            foreach (HistoryEntry entry in _summary.Entries)
            {
                double startRatio = (entry.StartedAt - _summary.StartedAt).TotalSeconds / _summary.TotalDuration.TotalSeconds;
                double endRatio = (entry.FinishedAt - _summary.StartedAt).TotalSeconds / _summary.TotalDuration.TotalSeconds;
                int left = Math.Max(0, Math.Min(ClientSize.Width, (int)Math.Round(startRatio * ClientSize.Width)));
                if (left >= ClientSize.Width) continue;
                int right = Math.Min(ClientSize.Width, Math.Max(left + 1, (int)Math.Round(endRatio * ClientSize.Width)));
                using (Brush brush = new SolidBrush(GetColor(entry.Kind)))
                    e.Graphics.FillRectangle(brush, left, bar.Top, right - left, bar.Height);
            }

            using (Brush text = new SolidBrush(SystemColors.ControlText))
            {
                e.Graphics.DrawString(_summary.StartedAt.ToString("HH:mm"), Font, text, 0, 35);
                string finish = _summary.FinishedAt.ToString("HH:mm");
                SizeF size = e.Graphics.MeasureString(finish, Font);
                e.Graphics.DrawString(finish, Font, text, ClientSize.Width - size.Width, 35);
            }
        }

        private static Color GetColor(ActivityKind kind)
        {
            if (kind == ActivityKind.Work) return Color.Firebrick;
            if (kind == ActivityKind.Meeting) return Color.MediumPurple;
            if (kind == ActivityKind.LongBreak) return Color.RoyalBlue;
            return Color.SeaGreen;
        }
    }
}
