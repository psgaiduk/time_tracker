using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TimeTracker.Application.Timer;
using TimeTracker.Application.Ports;
using TimeTracker.Domain.Timer;
using WinRT.Interop;

namespace TimeTracker.App.Features.Timer.Views;

public sealed partial class BreakOverlayWindow : Window
{
    private const int OverlayHeight = 64;
    private const uint WdaExcludeFromCapture = 0x11;
    private readonly TimerCoordinator _coordinator;
    private readonly IResourceService _resources;

    public BreakOverlayWindow(TimerCoordinator coordinator, IResourceService resources)
    {
        InitializeComponent();
        _coordinator = coordinator;
        _resources = resources;
        _coordinator.StateChanged += OnStateChanged;
        PositionOnPrimaryMonitor();
        SetWindowDisplayAffinity();
        Update(_coordinator.State);
    }

    private void OnStateChanged(object? sender, TimerState state) => Update(state);

    private void Update(TimerState state)
    {
        RemainingText.Text = Format(state.Remaining);
        SkipButton.Visibility = state.Phase == TimerPhase.AwaitingBreakDecision
            ? Visibility.Visible : Visibility.Collapsed;
        RestButton.Visibility = state.Phase == TimerPhase.AwaitingBreakDecision
            ? Visibility.Visible : Visibility.Collapsed;
        MessageText.Text = state.Phase switch
        {
            TimerPhase.AwaitingBreakDecision => _resources.Get("Break.WorkFinished"),
            TimerPhase.ShortBreak => _resources.Get("Break.Short"),
            TimerPhase.LongBreak => _resources.Get("Break.Long"),
            _ => _resources.Get("Timer.Working")
        };
        OverlayRoot.Background = new SolidColorBrush(WinUIColor(state.Phase));
    }

    private void SkipButton_OnClick(object sender, RoutedEventArgs e) => _coordinator.Skip();

    private void RestButton_OnClick(object sender, RoutedEventArgs e) => _coordinator.Rest();

    private void PositionOnPrimaryMonitor()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        var display = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        appWindow.MoveAndResize(new RectInt32(
            display.WorkArea.X,
            display.WorkArea.Y,
            display.WorkArea.Width,
            OverlayHeight));
        var presenter = OverlappedPresenter.Create();
        presenter.IsAlwaysOnTop = true;
        presenter.IsResizable = false;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        appWindow.SetPresenter(presenter);
    }

    private void SetWindowDisplayAffinity()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        _ = NativeMethods.SetWindowDisplayAffinity(hwnd, WdaExcludeFromCapture);
    }

    private static string Format(TimeSpan value) => $"{(int)value.TotalMinutes:00}:{value.Seconds:00}";

    private static Windows.UI.Color WinUIColor(TimerPhase phase) => phase switch
    {
        TimerPhase.AwaitingBreakDecision => ColorHelper.FromArgb(255, 185, 28, 28),
        TimerPhase.ShortBreak or TimerPhase.LongBreak => ColorHelper.FromArgb(255, 22, 101, 52),
        _ => ColorHelper.FromArgb(255, 51, 65, 85)
    };

    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowDisplayAffinity(nint hwnd, uint affinity);
    }
}
