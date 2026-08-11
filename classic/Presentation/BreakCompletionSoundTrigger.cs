using TimeTracker.Classic.Domain;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class BreakCompletionSoundTrigger
    {
        private bool _playedForCurrentBreak;

        internal bool ShouldPlay(TimerState state)
        {
            bool isBreak = state.Phase == TimerPhase.ShortBreak || state.Phase == TimerPhase.LongBreak;
            if (!isBreak)
            {
                _playedForCurrentBreak = false;
                return false;
            }

            if (state.Remaining > System.TimeSpan.Zero || _playedForCurrentBreak) return false;
            _playedForCurrentBreak = true;
            return true;
        }
    }
}
