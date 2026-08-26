using System.Collections.Generic;

namespace TimeTracker.Classic.Application
{
    internal interface IApplicationCatalog
    {
        IList<ApplicationChoice> GetRunningApplications();
    }
}
