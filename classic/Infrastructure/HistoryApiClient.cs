using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class HistoryApiClient
    {
        internal void UploadAsync(string url, string userId, DateTime day, IList<HistoryEntry> entries)
        {
            if (String.IsNullOrWhiteSpace(url) || String.IsNullOrWhiteSpace(userId)) return;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try { Upload(url, userId, day, entries); }
                catch (Exception) { }
            });
        }

        private static void Upload(string url, string userId, DateTime day, IList<HistoryEntry> entries)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.TrimEnd('/') + "/history");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            byte[] body = Encoding.UTF8.GetBytes(BuildJson(userId, day, entries));
            request.ContentLength = body.Length;
            using (Stream stream = request.GetRequestStream()) stream.Write(body, 0, body.Length);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) { }
        }

        private static string BuildJson(string userId, DateTime day, IList<HistoryEntry> entries)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{\"user_id\":\"").Append(Escape(userId)).Append("\",\"day\":\"");
            json.Append(day.ToString("yyyy-MM-dd")).Append("\",\"entries\":[");
            for (int i = 0; i < entries.Count; i++)
            {
                if (i > 0) json.Append(",");
                HistoryEntry entry = entries[i];
                json.Append("{\"kind\":\"").Append(entry.Kind).Append("\",\"started_at\":\"");
                json.Append(entry.StartedAt.ToString("o")).Append("\",\"finished_at\":\"");
                json.Append(entry.FinishedAt.ToString("o")).Append("\",\"planned_duration_seconds\":");
                json.Append(entry.PlannedDuration.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}");
            }
            return json.Append("]}").ToString();
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }
}
