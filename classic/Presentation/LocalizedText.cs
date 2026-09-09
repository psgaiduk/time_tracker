using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace TimeTracker.Classic.Presentation
{
    internal static class LocalizedText
    {
        private static readonly string _breakOverdue = Get("BreakOverdue");

        internal static string BreakOverdue
        {
            get { return _breakOverdue; }
        }

        internal static string FinishWorkDay { get { return Get("FinishWorkDay"); } }
        internal static string WorkDaySummaryTitle { get { return Get("WorkDaySummaryTitle"); } }
        internal static string WorkDayRangeFormat { get { return Get("WorkDayRangeFormat"); } }
        internal static string WorkDayTotalFormat { get { return Get("WorkDayTotalFormat"); } }
        internal static string WorkDayWorkFormat { get { return Get("WorkDayWorkFormat"); } }
        internal static string WorkDayTotalWorkFormat { get { return Get("WorkDayTotalWorkFormat"); } }
        internal static string WorkDayMeetingFormat { get { return Get("WorkDayMeetingFormat"); } }
        internal static string Meeting { get { return Get("Meeting"); } }
        internal static string WorkDayRestFormat { get { return Get("WorkDayRestFormat"); } }
        internal static string NoWorkDayActivity { get { return Get("NoWorkDayActivity"); } }
        internal static string NoWorkDayActivityForDateFormat { get { return Get("NoWorkDayActivityForDateFormat"); } }
        internal static string TotalWork { get { return Get("TotalWork"); } }
        internal static string Work { get { return Get("Work"); } }
        internal static string Meetings { get { return Get("Meetings"); } }
        internal static string TotalRest { get { return Get("TotalRest"); } }
        internal static string ShortBreaks { get { return Get("ShortBreaks"); } }
        internal static string LongBreaks { get { return Get("LongBreaks"); } }
        internal static string Close { get { return Get("Close"); } }
        internal static string Copy { get { return Get("Copy"); } }
        internal static string CopyFailed { get { return Get("CopyFailed"); } }
        internal static string PreviousDay { get { return Get("PreviousDay"); } }
        internal static string NextDay { get { return Get("NextDay"); } }
        internal static string WorkDayStart { get { return Get("WorkDayStart"); } }
        internal static string WorkDayEnd { get { return Get("WorkDayEnd"); } }
        internal static string AutomaticMeetingEnabled { get { return Get("AutomaticMeetingEnabled"); } }
        internal static string AddRunningApplication { get { return Get("AddRunningApplication"); } }
        internal static string ChooseExecutable { get { return Get("ChooseExecutable"); } }
        internal static string Remove { get { return Get("Remove"); } }
        internal static string RunningApplications { get { return Get("RunningApplications"); } }
        internal static string Add { get { return Get("Add"); } }
        internal static string Cancel { get { return Get("Cancel"); } }

        private static string Get(string key)
        {
            string resourceName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "StringsRu.resx" : "StringsEn.resx";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                XmlDocument document = new XmlDocument();
                document.Load(stream);
                XmlNode node = document.SelectSingleNode("/root/data[@name='" + key + "']/value");
                return node == null ? key : node.InnerText;
            }
        }
    }
}
