using System.Text.Json;
using TimeTracker.Application.Ports;
using TimeTracker.Application.Settings;

namespace TimeTracker.Infrastructure.Storage;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly PortableDataPathProvider _paths;

    public JsonSettingsStore(PortableDataPathProvider paths)
    {
        _paths = paths;
    }

    public AppSettings Load()
    {
        if (!File.Exists(_paths.SettingsPath))
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(_paths.SettingsPath);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(_paths.DataDirectory);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_paths.SettingsPath, json);
    }
}
