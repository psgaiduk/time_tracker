using Windows.ApplicationModel.Resources;
using TimeTracker.Application.Ports;

namespace TimeTracker.App.Resources;

public sealed class WinUiResourceService : IResourceService
{
    private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Resources");

    public string Get(string key) => _loader.GetString(key);
}
