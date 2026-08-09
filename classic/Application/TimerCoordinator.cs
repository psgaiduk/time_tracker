using System;
using TimeTracker.Classic.Domain;

namespace TimeTracker.Classic.Application
{
    internal sealed class TimerCoordinator
    {
        private readonly IClock _clock;
        private readonly TimerSession _session;

        internal TimerCoordinator(IClock clock, TimerRules rules)
        {
            _clock = clock;
            _session = new TimerSession(rules);
            State = _session.GetState(_clock.Now);
        }

        internal TimerState State { get; private set; }
        internal event EventHandler StateChanged;

        internal void Start() { _session.StartWork(_clock.Now); Publish(); }
        internal void Tick() { _session.Advance(_clock.Now); Publish(); }
        internal void Skip() { _session.Skip(_clock.Now); Publish(); }
        internal void Rest() { _session.Rest(_clock.Now); Publish(); }

        private void Publish()
        {
            State = _session.GetState(_clock.Now);
            EventHandler handler = StateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
