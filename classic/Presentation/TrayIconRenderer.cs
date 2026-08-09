using System;
using System.Drawing;
using System.Runtime.InteropServices;
using TimeTracker.Classic.Application;
using TimeTracker.Classic.Domain;

namespace TimeTracker.Classic.Presentation
{
    internal static class TrayIconRenderer
    {
        internal static string GetText(TimerState state, DailyWorkStats stats)
        {
            if (state.Phase != TimerPhase.Work && state.Phase != TimerPhase.AwaitingBreakDecision) return String.Empty;
            int minutes = (int)stats.ContinuousWork.TotalMinutes;
            return minutes > 99 ? "99+" : minutes.ToString();
        }

        internal static string GetKey(TimerState state, DailyWorkStats stats)
        {
            return state.Phase.ToString() + ":" + GetText(state, stats);
        }

        internal static Icon Create(TimerState state, DailyWorkStats stats)
        {
            Color background = Color.DimGray;
            if (state.Phase == TimerPhase.Work || state.Phase == TimerPhase.AwaitingBreakDecision) background = Color.Firebrick;
            if (state.Phase == TimerPhase.ShortBreak || state.Phase == TimerPhase.LongBreak) background = Color.SeaGreen;

            using (Bitmap bitmap = new Bitmap(16, 16))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                using (Brush backgroundBrush = new SolidBrush(background))
                    graphics.FillRectangle(backgroundBrush, 0, 1, 15, 14);
                string text = GetText(state, stats);
                if (text.Length > 0)
                {
                    float fontSize = text.Length > 2 ? 6.0f : 7.5f;
                    using (Font font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (Brush textBrush = new SolidBrush(Color.White))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        graphics.DrawString(text, font, textBrush, new RectangleF(0, 0, 16, 16), format);
                }
                IntPtr handle = bitmap.GetHicon();
                try { return (Icon)Icon.FromHandle(handle).Clone(); }
                finally { DestroyIcon(handle); }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}
