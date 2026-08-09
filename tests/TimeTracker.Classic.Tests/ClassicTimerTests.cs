using System;
using TimeTracker.Classic.Domain;
using TimeTracker.Classic.Presentation;

namespace TimeTracker.Classic.Tests
{
    internal static class ClassicTimerTests
    {
        private static int Main()
        {
            try
            {
                TestRulesUseSeconds();
                WorkCompletesByDeadline();
                TrayStatus_Work_ShowsRemainingTime();
                Console.WriteLine("Classic timer tests passed.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error.Message);
                return 1;
            }
        }

        private static void TestRulesUseSeconds()
        {
            TimerRules rules = TimerRules.Test();
            AssertEqual(TimeSpan.FromSeconds(25), rules.WorkDuration, "Test work duration");
            AssertEqual(TimeSpan.FromSeconds(5), rules.ShortBreakDuration, "Test short break duration");
            AssertEqual(TimeSpan.FromSeconds(90), rules.LongBreakDuration, "Test long break duration");
        }

        private static void WorkCompletesByDeadline()
        {
            DateTime start = new DateTime(2026, 1, 1, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Test());
            session.StartWork(start);
            session.Advance(start.AddSeconds(24));
            AssertEqual(TimerPhase.Work, session.GetState(start.AddSeconds(24)).Phase, "Phase before deadline");
            session.Advance(start.AddSeconds(25));
            AssertEqual(TimerPhase.AwaitingBreakDecision, session.GetState(start.AddSeconds(25)).Phase, "Phase at deadline");
        }

        private static void TrayStatus_Work_ShowsRemainingTime()
        {
            TimerState state = new TimerState(TimerPhase.Work, TimeSpan.FromMinutes(24).Add(TimeSpan.FromSeconds(59)), 0);
            AssertEqual("Работа — 24:59", TrayStatusText.Format(state), "Tray work status");
        }

        private static void AssertEqual(object expected, object actual, string name)
        {
            if (!Object.Equals(expected, actual))
                throw new InvalidOperationException(name + ": expected " + expected + ", actual " + actual + ".");
        }
    }
}
