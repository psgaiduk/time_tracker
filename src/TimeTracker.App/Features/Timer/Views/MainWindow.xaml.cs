using Microsoft.UI.Xaml;
using TimeTracker.Application.Timer;
using TimeTracker.App.Features.Timer;

namespace TimeTracker.App.Features.Timer.Views;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly BreakOverlayWindow _overlay;

    public MainWindow(TimerCoordinator coordinator, BreakOverlayWindow overlay)
    {
        InitializeComponent();
        ViewModel = new TimerViewModel(coordinator);
        _overlay = overlay;
        _overlay.Activate();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => coordinator.Tick();
        _timer.Start();
    }

    public TimerViewModel ViewModel { get; }
}
