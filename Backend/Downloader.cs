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
using System.Text.RegularExpressions;

namespace Backend
{

    public static class Downloader
    {
        // first data
        // http://oe3meta.orf.at/ApiV2.php/SongHistory.json?Res=200&DT=2017-04-28T08:00:00
        public static readonly DateTime FIRST_DATE_WITH_DATA = new(2017, 05, 28, 00, 00, 00);

        private static readonly string URL = @"http://oe3meta.orf.at/ApiV2.php/SongHistory.json?Res=200&DT={0}";

        public static List<JsonEvent> DownloadJsonEvents(DateTime dateTime)
        {
            Log.Information($"downloading {dateTime}");
            var json = DownloadJson(dateTime);
            var events = JsonToJsonEvents(json);
            Log.Information($"downloaded {events.Length} JsonEvents");

            // reverse order (by default the first song has the highest time)
            // use OrderBy instead of Reverse in case the service doesn't guarantee that
            var sortedEvents = events.OrderBy(e => e.Time).ToList();
            foreach (var e in sortedEvents)
                Log.Information($"{e.Time} {e.SongName} - {e.Artist}");

            return sortedEvents;
        }


        private static JsonEvent[] JsonToJsonEvents(string json)
        {
            if (json == null)
                return null;

            // datetime needs format 2021-12-19T18:22:27 (not 2021-12-19T18:22:27+0100)
            const string REPLACE_UTC_OFFSET_REGEX = @"(\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d)([+-]\d\d\d\d)";
            json = Regex.Replace(json, REPLACE_UTC_OFFSET_REGEX, "$1");

            return JsonSerializer.Deserialize<JsonEvent[]>(json, new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            });
        }


        private static string DownloadJson(DateTime dateTime)
        {
            var url = string.Format(URL, dateTime.ToString("yyyy-MM-ddTHH:mm:ss"));

            const string emptyResponse = "[]";
            var data = emptyResponse;

            var request = (HttpWebRequest)WebRequest.Create(url);
            using var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var receiveStream = response.GetResponseStream();
                StreamReader readStream;

                if (response.CharacterSet == null)
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                data = readStream.ReadToEnd();
                Log.Information($"downloaded json: {data}");
                readStream.Close();
            }

            if (data == emptyResponse)
            {
                Log.Error($"received no data statusCode={response.StatusCode} statusDescription={response.StatusDescription} url={url}");
                return null;
            }

            return data;
        }
    }
}
