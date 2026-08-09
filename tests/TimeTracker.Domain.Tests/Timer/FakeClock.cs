using TimeTracker.Application.Ports;

namespace TimeTracker.Domain.Tests.Timer;

internal sealed class FakeClock : IClock
{
    public FakeClock(DateTimeOffset now)
    {
        Now = now;
    }

    public DateTimeOffset Now { get; private set; }

    public void Advance(TimeSpan duration) => Now = Now.Add(duration);
}
