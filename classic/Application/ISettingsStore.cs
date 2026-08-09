namespace TimeTracker.Classic.Application
{
    internal interface ISettingsStore
    {
        AppSettings Load();
        void Save(AppSettings settings);
    }
}
