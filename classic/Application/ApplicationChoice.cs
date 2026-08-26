namespace TimeTracker.Classic.Application
{
    internal sealed class ApplicationChoice
    {
        internal ApplicationChoice(string name, string executablePath) { Name = name; ExecutablePath = executablePath; }
        internal string Name { get; private set; }
        internal string ExecutablePath { get; private set; }
        public override string ToString() { return Name + " — " + ExecutablePath; }
    }
}
