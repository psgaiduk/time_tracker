using System;
using TimeTracker.Classic.Domain;

namespace TimeTracker.Classic.Presentation
{
    internal static class TrayStatusText
    {
        internal static string Format(TimerState state)
        {
            string remaining = String.Format("{0:00}:{1:00}", (int)state.Remaining.TotalMinutes, state.Remaining.Seconds);
            if (state.Phase == TimerPhase.Work) return "Работа — " + remaining;
            if (state.Phase == TimerPhase.Meeting) return LocalizedText.Meeting;
            if (state.Phase == TimerPhase.ShortBreak) return "Короткий перерыв — " + remaining;
            if (state.Phase == TimerPhase.LongBreak) return "Большой перерыв — " + remaining;
            if (state.Phase == TimerPhase.AwaitingBreakDecision) return "Работа завершена";
            if (state.Phase == TimerPhase.WorkSummary) return "Заполни итоги работы";
            return "Начать работу";
        }
    }
}
