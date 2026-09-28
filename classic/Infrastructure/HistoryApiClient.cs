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
        internal void UploadAsync(string url, string userId, string token, DateTime day, IList<HistoryEntry> entries)
        {
            if (String.IsNullOrWhiteSpace(url) || String.IsNullOrWhiteSpace(userId) || String.IsNullOrWhiteSpace(token)) return;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try { Upload(url, userId, token, day, entries); }
                catch (Exception error) { Log(error); }
            });
        }

        private static void Upload(string url, string userId, string token, DateTime day, IList<HistoryEntry> entries)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.TrimEnd('/') + "/integration/history");
            request.Method = "POST";
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            request.ContentType = "application/json; charset=utf-8";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;
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

        private static void Log(Exception error)
        {
            try
            {
                string data = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                Directory.CreateDirectory(data);
                File.AppendAllText(Path.Combine(data, "api-sync.log"), DateTime.Now.ToString("o") + " " + error + Environment.NewLine);
            }
            catch (Exception) { }
        }
    }
}
