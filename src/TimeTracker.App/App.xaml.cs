using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using TimeTracker.Application.Ports;
using TimeTracker.Application.Timer;
using TimeTracker.Infrastructure.Clock;
using TimeTracker.App.Features.Timer.Views;
using TimeTracker.App.Resources;
using TimeTracker.Domain.Timer;
using TimeTracker.Infrastructure.Storage;

namespace TimeTracker.App;

public partial class App : Microsoft.UI.Xaml.Application
{
    private readonly ServiceProvider _services;
    private MainWindow? _mainWindow;

    public App()
    {
        InitializeComponent();

        var services = new ServiceCollection();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IResourceService, WinUiResourceService>();
        services.AddSingleton<PortableDataPathProvider>();
        services.AddSingleton<ISettingsStore, JsonSettingsStore>();
#if TEST_TIMER
        services.AddSingleton(TimerRules.Test);
#endif
        services.AddSingleton<TimerCoordinator>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<BreakOverlayWindow>();
        _services = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = _services.GetRequiredService<MainWindow>();
        _mainWindow.Activate();
    }
}
