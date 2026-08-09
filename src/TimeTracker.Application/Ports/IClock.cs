namespace TimeTracker.Application.Ports;

public interface IClock
{
    DateTimeOffset Now { get; }
}
