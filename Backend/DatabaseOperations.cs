using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class DatabaseOperations
    {
        public static void UpdateDb(DateTime? from=null, DateTime? till=null)
        {
            if(from == null)
                from = GetLatestEventDateTime();
            if(till == null)
                till = DateTime.Now;
            Log.Information($"updating database (from={from} till={till.Value})");

            var curTime = from;
            while (curTime < till && curTime < DateTime.Now)
            {
                var jsonEvents = Downloader.DownloadJsonEvents(curTime.Value);
                var newEventCount = JsonImporter.ImportJsonEvents(jsonEvents, till.Value);
                if (newEventCount == 0)
                {
                    Log.Information($"no new events for {curTime} (adding 30 minutes)");
                    curTime += TimeSpan.FromMinutes(30);
                }
                else
                {
                    curTime = GetLatestEventDateTime();
                    Log.Information($"updated database till {curTime}");
                }
            }
            Log.Information($"updated database (latest event was on {curTime})");
        }


        public static DateTime GetLatestEventDateTime()
        {
            using var db = new DatabaseContext();
            var latestStartTimeUnix = db.Events
                .OrderByDescending(e => e.StartTimeUnix)
                .FirstOrDefault()?.StartTimeUnix;

            if (latestStartTimeUnix != null)
                return Util.UnixTimestampToDateTime(latestStartTimeUnix.Value);
            else
                return Downloader.FIRST_DATE_WITH_DATA;
        }
    }
}
