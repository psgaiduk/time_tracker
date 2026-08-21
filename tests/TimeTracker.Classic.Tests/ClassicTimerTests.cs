using System;
using System.Collections.Generic;
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
                AppSettings_Defaults_ShowOverlayOnAllVirtualDesktops();
                BreakProgressWidth_DecreasesWithRemainingTime();
                BreakCompletionSoundTrigger_PlaysOnceAndResetsForNextBreak();
                WorkCompletesByDeadline();
                TrayStatus_Work_ShowsRemainingTime();
                AwaitingDecision_ContinuesCountingWorkUntilRest();
                StartShortBreakDuringWork_StartsShortBreakAndSavesWorkedTime();
                EndBreak_ShortBreak_StartsWorkImmediately();
                TrayIcon_Work_ShowsContinuousMinutes();
                FifthWork_Rest_StartsLongBreakAfterDecision();
                FifthWork_LongBreakDisabled_StartsShortBreak();
                Rest_SummaryPromptEnabled_WaitsBeforeBreak();
                BreakDeadline_WaitsForExplicitEndBreak();
                Work_AccruesShortAndLongBreakBalances();
                Rest_WithLongBalanceAtThreshold_StartsLongBreak();
                EndBreak_EarlyShortBreak_PreservesUnusedBalance();
                EndBreak_OverdueShortBreak_DeductsExcessFromLongBalance();
                EndBreak_EarlyLongBreak_PreservesUnusedLongBalanceAndClearsShort();
                BreakState_AfterDeadline_ShowsOverdueTime();
                CompleteWorkSummary_CountsSummaryTimeAsWork();
                EndBreak_RecordsTypedHistoryAndBalances();
                Coordinator_RestoresBalancesFromHistory();
                Stop_DuringWork_RecordsHistoryAndEarnedBalances();
                WorkDaySummary_CalculatesSpanWorkAndBreakTotals();
                FinishWorkDay_StopsCurrentPeriodAndReturnsSummary();
                Coordinator_OfflineTimeReducesBothBalancesIndependently();
                Meeting_SuppressesDeadlineUntilReturningToWork();
                Meeting_ReturnBeforeDeadline_ContinuesRegularWork();
                Meeting_Time_AccruesBreakAndIsRecordedSeparately();
                WorkDaySummary_SeparatesFocusedWorkMeetingsAndTotalWork();
                TrayIcon_Meeting_IsPurpleAndShowsContinuousMinutes();
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

        private static void AppSettings_Defaults_ShowOverlayOnAllVirtualDesktops()
        {
            AppSettings settings = new AppSettings();
            AssertEqual(true, settings.ShowOverlayOnAllVirtualDesktops, "Overlay is shown on all virtual desktops by default");
        }

        private static void BreakProgressWidth_DecreasesWithRemainingTime()
        {
            AssertEqual(1000, BreakProgressWidth.Calculate(1000, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)), "Full break progress width");
            AssertEqual(500, BreakProgressWidth.Calculate(1000, TimeSpan.FromMinutes(2.5), TimeSpan.FromMinutes(5)), "Half break progress width");
            AssertEqual(0, BreakProgressWidth.Calculate(1000, TimeSpan.Zero, TimeSpan.FromMinutes(5)), "Finished break progress width");
        }

        private static void BreakCompletionSoundTrigger_PlaysOnceAndResetsForNextBreak()
        {
            BreakCompletionSoundTrigger trigger = new BreakCompletionSoundTrigger();
            AssertEqual(false, trigger.ShouldPlay(new TimerState(TimerPhase.ShortBreak, TimeSpan.FromSeconds(1), 0)), "No sound before break deadline");
            AssertEqual(true, trigger.ShouldPlay(new TimerState(TimerPhase.ShortBreak, TimeSpan.Zero, 0)), "Sound at break deadline");
            AssertEqual(false, trigger.ShouldPlay(new TimerState(TimerPhase.ShortBreak, TimeSpan.Zero, 0)), "No repeated sound after break deadline");
            AssertEqual(false, trigger.ShouldPlay(new TimerState(TimerPhase.Work, TimeSpan.FromSeconds(25), 0)), "Reset outside break");
            AssertEqual(true, trigger.ShouldPlay(new TimerState(TimerPhase.LongBreak, TimeSpan.Zero, 0)), "Sound for next break");
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

        private static void StartShortBreakDuringWork_StartsShortBreakAndSavesWorkedTime()
        {
            DateTime start = new DateTime(2026, 8, 15, 9, 0, 0);
            FakeClock clock = new FakeClock(start);
            FakeHistoryStore history = new FakeHistoryStore();
            TimerCoordinator coordinator = new TimerCoordinator(clock, TimerRules.Test(), history);
            coordinator.Start();
            clock.Now = start.AddSeconds(10);

            coordinator.StartShortBreak();

            AssertEqual(TimerPhase.ShortBreak, coordinator.State.Phase, "Short break after tray action during work");
            AssertEqual(TimeSpan.FromSeconds(2), coordinator.State.Remaining, "Accumulated short break duration after tray action");
            AssertEqual(0, coordinator.State.CompletedWorkIntervals, "Interrupted work is not a completed interval");
            AssertEqual(TimeSpan.FromSeconds(10), history.Total, "Worked time saved before short break");
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
                    session.EndBreak(now);
                }
            }

            AssertEqual(TimerPhase.AwaitingBreakDecision, session.GetState(now).Phase, "Fifth work decision phase");
            AssertEqual(5, session.GetState(now).CompletedWorkIntervals, "Completed count before long break");
            session.Rest(now);
            AssertEqual(TimerPhase.LongBreak, session.GetState(now).Phase, "Long break after rest choice");
            AssertEqual(TimeSpan.FromSeconds(62.5), session.GetState(now).Remaining, "Accumulated long break duration");
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
                    session.EndBreak(now);
                }
            }
            session.Rest(now);
            AssertEqual(TimerPhase.ShortBreak, session.GetState(now).Phase, "Short break when long break disabled");
        }

        private static void Rest_SummaryPromptEnabled_WaitsBeforeBreak()
        {
            TimerRules rules = TimerRules.Test(delegate(DateTime date) { return true; }, delegate { return true; });
            TimerSession session = new TimerSession(rules);
            DateTime now = new DateTime(2026, 8, 10, 9, 0, 0);
            session.StartWork(now);
            now = now.AddSeconds(25);
            session.Advance(now);
            session.Rest(now);
            AssertEqual(TimerPhase.WorkSummary, session.GetState(now).Phase, "Summary phase after rest choice");
            session.CompleteWorkSummary(now);
            AssertEqual(TimerPhase.ShortBreak, session.GetState(now).Phase, "Break after summary completion");
        }

        private static void BreakDeadline_WaitsForExplicitEndBreak()
        {
            DateTime start = new DateTime(2026, 8, 10, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Test());
            session.StartWork(start);
            session.Advance(start.AddSeconds(25));
            session.Rest(start.AddSeconds(25));
            session.Advance(start.AddSeconds(30));
            AssertEqual(TimerPhase.ShortBreak, session.GetState(start.AddSeconds(30)).Phase, "Break phase at deadline");
            session.EndBreak(start.AddSeconds(31));
            AssertEqual(TimerPhase.Work, session.GetState(start.AddSeconds(31)).Phase, "Work after explicit end break");
        }

        private static void Work_AccruesShortAndLongBreakBalances()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.Advance(start.AddMinutes(25));
            session.Rest(start.AddMinutes(25));
            TimerState state = session.GetState(start.AddMinutes(25));
            AssertEqual(TimeSpan.FromMinutes(5), state.ShortBreakBalance, "Short balance after 25 work minutes");
            AssertEqual(TimeSpan.FromMinutes(12.5), state.LongBreakBalance, "Long balance after 25 work minutes");
            AssertEqual(TimeSpan.FromMinutes(5), state.Remaining, "Accumulated short break duration");
        }

        private static void Rest_WithLongBalanceAtThreshold_StartsLongBreak()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.Advance(start.AddMinutes(110));
            session.Rest(start.AddMinutes(110));
            TimerState state = session.GetState(start.AddMinutes(110));
            AssertEqual(TimerPhase.LongBreak, state.Phase, "Long break at threshold");
            AssertEqual(TimeSpan.Zero, state.ShortBreakBalance, "Long break clears short balance");
            AssertEqual(TimeSpan.FromMinutes(55), state.Remaining, "Accumulated long break duration");
        }

        private static void EndBreak_EarlyShortBreak_PreservesUnusedBalance()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.Advance(start.AddMinutes(25));
            session.Rest(start.AddMinutes(25));
            session.EndBreak(start.AddMinutes(28));
            AssertEqual(TimeSpan.FromMinutes(2), session.GetState(start.AddMinutes(28)).ShortBreakBalance, "Unused short balance");
        }

        private static void EndBreak_OverdueShortBreak_DeductsExcessFromLongBalance()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.Advance(start.AddMinutes(25));
            session.Rest(start.AddMinutes(25));
            session.EndBreak(start.AddMinutes(31));
            TimerState state = session.GetState(start.AddMinutes(31));
            AssertEqual(TimeSpan.Zero, state.ShortBreakBalance, "Used short balance");
            AssertEqual(TimeSpan.FromMinutes(12), state.LongBreakBalance, "Only excess beyond ten percent deducted");
        }

        private static void EndBreak_EarlyLongBreak_PreservesUnusedLongBalanceAndClearsShort()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.Advance(start.AddMinutes(125));
            session.Rest(start.AddMinutes(125));
            session.EndBreak(start.AddMinutes(165));
            TimerState state = session.GetState(start.AddMinutes(165));
            AssertEqual(TimeSpan.Zero, state.ShortBreakBalance, "Long break cleared short balance");
            AssertEqual(TimeSpan.FromMinutes(22.5), state.LongBreakBalance, "Unused long balance");
        }

        private static void BreakState_AfterDeadline_ShowsOverdueTime()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.Advance(start.AddMinutes(25));
            session.Rest(start.AddMinutes(25));
            TimerState state = session.GetState(start.AddMinutes(31));
            AssertEqual(TimeSpan.FromMinutes(1), state.Overdue, "Break overdue duration");
        }

        private static void CompleteWorkSummary_CountsSummaryTimeAsWork()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            FakeClock clock = new FakeClock(start);
            FakeHistoryStore history = new FakeHistoryStore();
            TimerCoordinator coordinator = new TimerCoordinator(clock, TimerRules.Test(delegate(DateTime date) { return true; }, delegate { return true; }), history);
            coordinator.Start();
            clock.Now = start.AddSeconds(25);
            coordinator.Tick();
            coordinator.Rest();
            clock.Now = start.AddSeconds(35);
            coordinator.CompleteWorkSummary();
            AssertEqual(TimeSpan.FromSeconds(35), history.Total, "Summary time saved as work");
            AssertEqual(TimeSpan.FromSeconds(7), coordinator.State.Remaining, "Summary time accrued short rest");
        }

        private static void EndBreak_RecordsTypedHistoryAndBalances()
        {
            DateTime start = new DateTime(2026, 8, 19, 9, 0, 0);
            FakeClock clock = new FakeClock(start);
            FakeHistoryStore history = new FakeHistoryStore();
            TimerCoordinator coordinator = new TimerCoordinator(clock, TimerRules.Default(), history);
            coordinator.Start();
            clock.Now = start.AddMinutes(25);
            coordinator.Tick();
            coordinator.Rest();
            clock.Now = start.AddMinutes(28);
            coordinator.EndBreak();
            AssertEqual(2, history.Entries.Count, "Work and break history entries");
            AssertEqual(ActivityKind.Work, history.Entries[0].Kind, "Work history kind");
            AssertEqual(ActivityKind.ShortBreak, history.Entries[1].Kind, "Break history kind");
            AssertEqual(TimeSpan.FromMinutes(5), history.Entries[1].PlannedDuration, "Planned break in history");
            AssertEqual(TimeSpan.FromMinutes(2), history.GetLatestBalances().ShortBreak, "Persisted unused short balance");
        }

        private static void Coordinator_RestoresBalancesFromHistory()
        {
            FakeHistoryStore history = new FakeHistoryStore(new BreakBalances(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(12)));
            TimerCoordinator coordinator = new TimerCoordinator(new FakeClock(new DateTime(2026, 8, 19, 12, 0, 0)), TimerRules.Default(), history);
            AssertEqual(TimeSpan.FromMinutes(2), coordinator.State.ShortBreakBalance, "Restored short balance");
            AssertEqual(TimeSpan.FromMinutes(12), coordinator.State.LongBreakBalance, "Restored long balance");
        }

        private static void Stop_DuringWork_RecordsHistoryAndEarnedBalances()
        {
            DateTime start = new DateTime(2026, 8, 19, 12, 0, 0);
            FakeClock clock = new FakeClock(start);
            FakeHistoryStore history = new FakeHistoryStore();
            TimerCoordinator coordinator = new TimerCoordinator(clock, TimerRules.Default(), history);
            coordinator.Start();
            clock.Now = start.AddMinutes(10);
            coordinator.Stop();
            AssertEqual(TimeSpan.FromMinutes(10), history.Total, "Work saved on application exit");
            AssertEqual(TimeSpan.FromMinutes(2), history.GetLatestBalances().ShortBreak, "Short balance saved on application exit");
            AssertEqual(TimeSpan.FromMinutes(5), history.GetLatestBalances().LongBreak, "Long balance saved on application exit");
        }

        private static void WorkDaySummary_CalculatesSpanWorkAndBreakTotals()
        {
            DateTime day = new DateTime(2026, 8, 20);
            List<HistoryEntry> entries = new List<HistoryEntry>();
            entries.Add(new HistoryEntry(ActivityKind.Work, day.AddHours(9), day.AddHours(10), TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero));
            entries.Add(new HistoryEntry(ActivityKind.ShortBreak, day.AddHours(10), day.AddHours(10).AddMinutes(15), TimeSpan.FromMinutes(15), TimeSpan.Zero, TimeSpan.Zero));
            entries.Add(new HistoryEntry(ActivityKind.Work, day.AddHours(10).AddMinutes(30), day.AddHours(12), TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero));
            WorkDaySummary summary = WorkDaySummary.Create(entries, day.AddHours(12));
            AssertEqual(day.AddHours(9), summary.StartedAt, "Work day start");
            AssertEqual(TimeSpan.FromHours(3), summary.TotalDuration, "Work day span");
            AssertEqual(TimeSpan.FromHours(2.5), summary.WorkDuration, "Work total");
            AssertEqual(TimeSpan.FromMinutes(15), summary.BreakDuration, "Break total");
        }

        private static void FinishWorkDay_StopsCurrentPeriodAndReturnsSummary()
        {
            DateTime start = new DateTime(2026, 8, 20, 9, 0, 0);
            FakeClock clock = new FakeClock(start);
            FakeHistoryStore history = new FakeHistoryStore();
            TimerCoordinator coordinator = new TimerCoordinator(clock, TimerRules.Default(), history);
            coordinator.Start();
            clock.Now = start.AddMinutes(40);
            WorkDaySummary summary = coordinator.FinishWorkDay();
            AssertEqual(TimerPhase.Idle, coordinator.State.Phase, "Idle after finishing work day");
            AssertEqual(TimeSpan.FromMinutes(40), summary.TotalDuration, "Finished day duration");
            AssertEqual(TimeSpan.FromMinutes(40), summary.WorkDuration, "Finished day work");
            AssertEqual(1, summary.Entries.Count, "Finished day timeline entry");
        }

        private static void Coordinator_OfflineTimeReducesBothBalancesIndependently()
        {
            DateTime lastActivity = new DateTime(2026, 8, 19, 18, 0, 0);
            FakeHistoryStore history = new FakeHistoryStore(
                new BreakBalances(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(40)), lastActivity);
            TimerCoordinator coordinator = new TimerCoordinator(
                new FakeClock(lastActivity.AddMinutes(20)), TimerRules.Default(), history);
            AssertEqual(TimeSpan.Zero, coordinator.State.ShortBreakBalance, "Offline time clears smaller short balance");
            AssertEqual(TimeSpan.FromMinutes(20), coordinator.State.LongBreakBalance, "Offline time independently reduces long balance");
        }

        private static void Meeting_SuppressesDeadlineUntilReturningToWork()
        {
            DateTime start = new DateTime(2026, 8, 20, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.StartMeeting(start.AddMinutes(10));
            session.Advance(start.AddMinutes(40));
            AssertEqual(TimerPhase.Meeting, session.GetState(start.AddMinutes(40)).Phase, "Meeting ignores work deadline");
            session.EndMeeting(start.AddMinutes(40));
            AssertEqual(TimerPhase.AwaitingBreakDecision, session.GetState(start.AddMinutes(40)).Phase, "Returning after deadline shows decision");
        }

        private static void Meeting_ReturnBeforeDeadline_ContinuesRegularWork()
        {
            DateTime start = new DateTime(2026, 8, 20, 9, 0, 0);
            TimerSession session = new TimerSession(TimerRules.Default());
            session.StartWork(start);
            session.StartMeeting(start.AddMinutes(5));
            session.EndMeeting(start.AddMinutes(15));
            TimerState state = session.GetState(start.AddMinutes(15));
            AssertEqual(TimerPhase.Work, state.Phase, "Returning before deadline resumes work");
            AssertEqual(TimeSpan.FromMinutes(10), state.Remaining, "Original work deadline is retained");
        }

        private static void Meeting_Time_AccruesBreakAndIsRecordedSeparately()
        {
            DateTime start = new DateTime(2026, 8, 20, 9, 0, 0);
            FakeClock clock = new FakeClock(start);
            FakeHistoryStore history = new FakeHistoryStore();
            TimerCoordinator coordinator = new TimerCoordinator(clock, TimerRules.Default(), history);
            coordinator.Start();
            clock.Now = start.AddMinutes(5);
            coordinator.ToggleMeeting();
            clock.Now = start.AddMinutes(25);
            coordinator.StartShortBreak();
            AssertEqual(ActivityKind.Work, history.Entries[0].Kind, "Focused work history kind");
            AssertEqual(ActivityKind.Meeting, history.Entries[1].Kind, "Meeting history kind");
            AssertEqual(TimeSpan.FromMinutes(5), coordinator.State.Remaining, "Meeting accrues break like work");
        }

        private static void WorkDaySummary_SeparatesFocusedWorkMeetingsAndTotalWork()
        {
            DateTime start = new DateTime(2026, 8, 20, 9, 0, 0);
            List<HistoryEntry> entries = new List<HistoryEntry>();
            entries.Add(new HistoryEntry(ActivityKind.Work, start, start.AddHours(2), TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero));
            entries.Add(new HistoryEntry(ActivityKind.Meeting, start.AddHours(2), start.AddHours(3), TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero));
            entries.Add(new HistoryEntry(ActivityKind.ShortBreak, start.AddHours(3), start.AddHours(3.25), TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero));
            entries.Add(new HistoryEntry(ActivityKind.LongBreak, start.AddHours(3.25), start.AddHours(3.5), TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero));
            WorkDaySummary summary = WorkDaySummary.Create(entries, start.AddHours(3.5));
            AssertEqual(TimeSpan.FromHours(3), summary.TotalWorkDuration, "Total work includes meetings");
            AssertEqual(TimeSpan.FromHours(2), summary.WorkDuration, "Focused work total");
            AssertEqual(TimeSpan.FromHours(1), summary.MeetingDuration, "Meeting total");
            AssertEqual(TimeSpan.FromMinutes(30), summary.BreakDuration, "Rest total excludes meetings");
            AssertEqual(TimeSpan.FromMinutes(15), summary.ShortBreakDuration, "Short break total");
            AssertEqual(TimeSpan.FromMinutes(15), summary.LongBreakDuration, "Long break total");
        }

        private static void TrayIcon_Meeting_IsPurpleAndShowsContinuousMinutes()
        {
            TimerState state = new TimerState(TimerPhase.Meeting, TimeSpan.Zero, 0);
            DailyWorkStats stats = new DailyWorkStats(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(35), TimeSpan.FromMinutes(35));
            AssertEqual("35", TrayIconRenderer.GetText(state, stats), "Meeting tray minutes");
            AssertEqual(System.Drawing.Color.MediumPurple, TrayIconRenderer.GetBackgroundColor(state), "Meeting tray color");
        }

        private sealed class FakeClock : IClock
        {
            internal FakeClock(DateTime now) { Now = now; }
            public DateTime Now { get; set; }
        }

        private sealed class FakeHistoryStore : IWorkHistoryStore
        {
            internal TimeSpan Total;
            internal readonly List<HistoryEntry> Entries = new List<HistoryEntry>();
            private BreakBalances _balances = new BreakBalances(TimeSpan.Zero, TimeSpan.Zero);
            private DateTime? _latestFinishedAt;
            internal FakeHistoryStore() { }
            internal FakeHistoryStore(BreakBalances balances) { _balances = balances; }
            internal FakeHistoryStore(BreakBalances balances, DateTime latestFinishedAt) { _balances = balances; _latestFinishedAt = latestFinishedAt; }
            public void Add(HistoryEntry entry)
            {
                if (entry.Kind == ActivityKind.Work || entry.Kind == ActivityKind.Meeting) Total += entry.FinishedAt - entry.StartedAt;
                Entries.Add(entry);
                _balances = new BreakBalances(entry.ShortBreakBalance, entry.LongBreakBalance);
                _latestFinishedAt = entry.FinishedAt;
            }
            public TimeSpan GetTotal(DateTime day) { return Total; }
            public BreakBalances GetLatestBalances() { return _balances; }
            public DateTime? GetLatestFinishedAt() { return _latestFinishedAt; }
            public IList<HistoryEntry> GetEntries(DateTime day)
            {
                List<HistoryEntry> result = new List<HistoryEntry>();
                DateTime from = day.Date;
                DateTime to = from.AddDays(1);
                foreach (HistoryEntry entry in Entries)
                    if (entry.FinishedAt > from && entry.StartedAt < to) result.Add(entry);
                return result;
            }
        }

        private static void AssertEqual(object expected, object actual, string name)
        {
            if (!Object.Equals(expected, actual))
                throw new InvalidOperationException(name + ": expected " + expected + ", actual " + actual + ".");
        }
    }
}
