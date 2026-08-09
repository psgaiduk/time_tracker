namespace TimeTracker.Infrastructure.Storage;

public sealed class PortableDataPathProvider
{
    public string DataDirectory => Path.Combine(AppContext.BaseDirectory, "data");

    public string SettingsPath => Path.Combine(DataDirectory, "settings.json");
}
