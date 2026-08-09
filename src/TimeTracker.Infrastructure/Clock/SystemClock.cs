using TimeTracker.Application.Ports;

namespace TimeTracker.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
