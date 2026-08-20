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
        internal static string WorkDayLegend { get { return Get("WorkDayLegend"); } }
        internal static string Close { get { return Get("Close"); } }

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
