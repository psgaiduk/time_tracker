using System;
using TimeTracker.Classic.Domain;
using TimeTracker.Classic.Presentation;
using TimeTracker.Classic.Application;

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
                AwaitingDecision_ContinuesCountingWorkUntilRest();
                EndBreak_ShortBreak_StartsWorkImmediately();
                TrayIcon_Work_ShowsContinuousMinutes();
                FifthWork_Rest_StartsLongBreakAfterDecision();
                FifthWork_LongBreakDisabled_StartsShortBreak();
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

        private static void AwaitingDecision_ContinuesCountingWorkUntilRest()
        {
            DateTime start = new DateTime(2026, 8, 9, 9, 0, 0);
            FakeClock clock = new FakeClock(start);
            FakeHistoryStore history = new FakeHistoryStore();
            TimerCoordinator coordinator = new TimerCoordinator(clock, TimerRules.Test(), history);
            coordinator.Start();
            clock.Now = start.AddSeconds(35);
            coordinator.Tick();
            AssertEqual(TimeSpan.FromSeconds(35), coordinator.Stats.CurrentPeriod, "Current period during decision");
            AssertEqual(TimeSpan.FromSeconds(35), coordinator.Stats.ContinuousWork, "Continuous work during decision");
            AssertEqual(TimeSpan.FromSeconds(35), coordinator.Stats.WorkedToday, "Today during decision");
            coordinator.Rest();
            AssertEqual(TimeSpan.Zero, coordinator.Stats.ContinuousWork, "Continuous work after rest");
            AssertEqual(TimeSpan.FromSeconds(35), history.Total, "Saved work duration");
        }

        private static void EndBreak_ShortBreak_StartsWorkImmediately()
        {
            DateTime start = new DateTime(2026, 8, 9, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Test());
            session.StartWork(start);
            session.Advance(start.AddSeconds(25));
            session.Rest(start.AddSeconds(25));

            session.EndBreak(start.AddSeconds(27));

            TimerState state = session.GetState(start.AddSeconds(27));
            AssertEqual(TimerPhase.Work, state.Phase, "Phase after ending break");
            AssertEqual(TimeSpan.FromSeconds(25), state.Remaining, "Work duration after ending break");
        }

        private static void TrayIcon_Work_ShowsContinuousMinutes()
        {
            TimerState state = new TimerState(TimerPhase.Work, TimeSpan.FromMinutes(10), 0);
            DailyWorkStats stats = new DailyWorkStats(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(47), TimeSpan.FromMinutes(80));
            AssertEqual("47", TrayIconRenderer.GetText(state, stats), "Tray icon work text");
            state = new TimerState(TimerPhase.ShortBreak, TimeSpan.FromMinutes(3), 0);
            AssertEqual(String.Empty, TrayIconRenderer.GetText(state, stats), "Tray icon break text");
        }

        private static void FifthWork_Rest_StartsLongBreakAfterDecision()
        {
            TimerSession session = new TimerSession(TimerRules.Test());
            DateTime now = new DateTime(2026, 8, 9, 9, 0, 0);
            session.StartWork(now);
            for (int interval = 1; interval <= 5; interval++)
            {
                now = now.AddSeconds(25);
                session.Advance(now);
                if (interval < 5)
                {
                    session.Rest(now);
                    now = now.AddSeconds(5);
                    session.Advance(now);
                }
            }

            AssertEqual(TimerPhase.AwaitingBreakDecision, session.GetState(now).Phase, "Fifth work decision phase");
            AssertEqual(5, session.GetState(now).CompletedWorkIntervals, "Completed count before long break");
            session.Rest(now);
            AssertEqual(TimerPhase.LongBreak, session.GetState(now).Phase, "Long break after rest choice");
            AssertEqual(TimeSpan.FromSeconds(90), session.GetState(now).Remaining, "Long break duration");
        }

        private static void FifthWork_LongBreakDisabled_StartsShortBreak()
        {
            TimerRules rules = TimerRules.Test(delegate(DateTime date) { return false; });
            TimerSession session = new TimerSession(rules);
            DateTime now = new DateTime(2026, 8, 9, 9, 0, 0);
            session.StartWork(now);
            for (int interval = 1; interval <= 5; interval++)
            {
                now = now.AddSeconds(25);
                session.Advance(now);
                if (interval < 5)
                {
                    session.Rest(now);
                    now = now.AddSeconds(5);
                    session.Advance(now);
                }
            }
            session.Rest(now);
            AssertEqual(TimerPhase.ShortBreak, session.GetState(now).Phase, "Short break when long break disabled");
        }

        private sealed class FakeClock : IClock
        {
            internal FakeClock(DateTime now) { Now = now; }
            public DateTime Now { get; set; }
        }

        private sealed class FakeHistoryStore : IWorkHistoryStore
        {
            internal TimeSpan Total;
            public void Add(DateTime startedAt, DateTime finishedAt) { Total += finishedAt - startedAt; }
            public TimeSpan GetTotal(DateTime day) { return Total; }
        }

        private static void AssertEqual(object expected, object actual, string name)
        {
            if (!Object.Equals(expected, actual))
                throw new InvalidOperationException(name + ": expected " + expected + ", actual " + actual + ".");
        }
    }
}
