using System.Text.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json.Serialization;

namespace Backend
{

    public static class Downloader
    {
        // first data
        // http://oe3meta.orf.at/ApiV2.php/SongHistory.json?Res=200&DT=2017-04-28T08:00:00
        private static readonly DateTime firstDate = new(2017, 04, 28, 08, 00, 00);

        private static readonly string URL = @"http://oe3meta.orf.at/ApiV2.php/SongHistory.json?Res=200&DT={0}";

        public static List<JsonEvent> DownloadFrom(DateTime? paramDateTime)
        {
            return DownloadFromTill(paramDateTime, DateTime.Now);
        }

        public static List<JsonEvent> DownloadFromTill(DateTime? from, DateTime? till)
        {
            DateTime dateTime;
            if (from == null)
                dateTime = firstDate;
            else
                dateTime = from.Value;


            var list = new List<JsonEvent>();
            while (dateTime < till)
            {
                var events = Download(dateTime)?.ToList();
                if (events != null)
                {
                    events.OrderBy(e => e.Time).ToList()
                    .ForEach(e =>
                    {
                        if (e.Time > dateTime && !list.Contains(e))
                            list.Add(e);
                    });
                }


                DateTime? newDateTime = null;
                if (list.LastOrDefault() != null)
                    newDateTime = list.LastOrDefault().Time;

                if (newDateTime != null && newDateTime > dateTime)
                    dateTime = newDateTime.Value;
                else
                    dateTime = dateTime.AddMinutes(5);

                if (list.Count % 100 == 0)
                    Log.Information($"Downloaded till {Util.DateTimeToString(dateTime)}");
            }

            return list.GroupBy(je => je.Time).Select(g => g.First()).ToList(); // sometime the service returns a lot of duplciates
        }

        private static JsonEvent[] Download(DateTime dateTime)
        {
            var json = DownloadJson(dateTime);
            json = TidyUpJson(json);
            var events = JsonToJsonEvents(json);

            return events;
        }
        private static string DownloadJson(DateTime dateTime)
        {
            var url = string.Format(URL, Util.DateTimeToString(dateTime));
            return GetJsonFromUrl(url);
        }

        private static string TidyUpJson(string json)
        {
            if (json == null) return null;

            // sometimes weird stuff is in id field
            var curIdx = 0;
            while (true)
            {
                const string ID_SEARCH_STRING = "\"Id\":\"";
                var idStartIdxRelative = json[curIdx..].IndexOf(ID_SEARCH_STRING);
                if (idStartIdxRelative == -1) break;
                var idStartIdx = idStartIdxRelative + curIdx;


                var idEndIdxRelative = json[(idStartIdx + ID_SEARCH_STRING.Length + 1)..].IndexOf('\"');
                if (idEndIdxRelative == -1)
                {
                    Log.Error($"failed to retrieve id value (json={json}, idStartIdx={idStartIdx}, idEndIdxRelative={idEndIdxRelative})");
                    break;
                }
                var idEndIdx = idEndIdxRelative + idStartIdx + ID_SEARCH_STRING.Length + 1;

                var idValue = json[(idStartIdx + ID_SEARCH_STRING.Length)..idEndIdx];
                var tidyIdValue = new string(idValue.Where(c => char.IsDigit(c)).ToArray());

                json = json[..(idStartIdx + ID_SEARCH_STRING.Length)] + tidyIdValue + json[idEndIdx..];
                curIdx = idStartIdx + ID_SEARCH_STRING.Length + tidyIdValue.Length + 1;
            }

            return json;
        }


        private static JsonEvent[] JsonToJsonEvents(string json)
        {
            if (json == null)
                return null;

            // datetime needs format 2021-12-19T18:22:27+0100
            json = json.Replace("+0100\",", "\",");
            return JsonSerializer.Deserialize<JsonEvent[]>(json, new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            });
        }


        private static readonly string emptyResponse = "[]";
        private static string GetJsonFromUrl(string url)
        {
            // copy empty response
            var data = "" + emptyResponse;

            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var receiveStream = response.GetResponseStream();
                StreamReader readStream;

                if (response.CharacterSet == null)
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            if (data == emptyResponse)
            {
                Log.Error($"{response.StatusCode} {response.StatusDescription}");
                return null;
            }


            return data;
        }
    }
}
