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
