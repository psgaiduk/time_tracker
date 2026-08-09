using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application.Timer;
using TimeTracker.Domain.Timer;

namespace TimeTracker.App.Features.Timer;

public partial class TimerViewModel : ObservableObject
{
    private readonly TimerCoordinator _coordinator;
    private int _completedWorkIntervals;
    private TimerPhase _phase = TimerPhase.Idle;
    private string _remainingText = "25:00";

    public string RemainingText
    {
        get => _remainingText;
        private set => SetProperty(ref _remainingText, value);
    }

    public TimerPhase Phase
    {
        get => _phase;
        private set => SetProperty(ref _phase, value);
    }

    public int CompletedWorkIntervals
    {
        get => _completedWorkIntervals;
        private set => SetProperty(ref _completedWorkIntervals, value);
    }

    public TimerViewModel(TimerCoordinator coordinator)
    {
        _coordinator = coordinator;
        _coordinator.StateChanged += OnStateChanged;
        Apply(_coordinator.State);
    }

    public bool IsIdle => Phase == TimerPhase.Idle;
    public bool IsAwaitingBreakDecision => Phase == TimerPhase.AwaitingBreakDecision;
    public bool IsBreak => Phase is TimerPhase.ShortBreak or TimerPhase.LongBreak;
    public bool CanStart => IsIdle;
    public bool CanSkip => IsAwaitingBreakDecision;
    public bool CanRest => IsAwaitingBreakDecision;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start() => _coordinator.Start();

    [RelayCommand(CanExecute = nameof(CanSkip))]
    private void Skip() => _coordinator.Skip();

    [RelayCommand(CanExecute = nameof(CanRest))]
    private void Rest() => _coordinator.Rest();

    private void OnStateChanged(object? sender, TimerState state) => Apply(state);

    private void Apply(TimerState state)
    {
        Phase = state.Phase;
        RemainingText = $"{(int)state.Remaining.TotalMinutes:00}:{state.Remaining.Seconds:00}";
        CompletedWorkIntervals = state.CompletedWorkIntervals;
        OnPropertyChanged(nameof(IsIdle));
        OnPropertyChanged(nameof(IsAwaitingBreakDecision));
        OnPropertyChanged(nameof(IsBreak));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanSkip));
        OnPropertyChanged(nameof(CanRest));
        StartCommand.NotifyCanExecuteChanged();
        SkipCommand.NotifyCanExecuteChanged();
        RestCommand.NotifyCanExecuteChanged();
    }
}
