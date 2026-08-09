using Microsoft.UI.Xaml;

namespace TimeTracker.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.Start(_ =>
        {
            var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            _ = new App();
        });
    }
}
