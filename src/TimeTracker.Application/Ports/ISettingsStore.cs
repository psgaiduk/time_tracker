using TimeTracker.Application.Settings;

namespace TimeTracker.Application.Ports;

public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
