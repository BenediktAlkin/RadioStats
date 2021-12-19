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
        public static void UpdateDb(DateTime? till=null)
        {
            using var db = new DatabaseContext();
            var latestDate = GetLatestEventDateTime();
            var from = new DateTime(latestDate.Year, latestDate.Month, latestDate.Day);
            if(till == null)
                till = DateTime.Now;
            Log.Information($"updating database (from={Util.DateTimeToString(from)} till={Util.DateTimeToString(till.Value)})");

            while (from < till && from < DateTime.Now)
            {
                from = from.AddDays(1);
                Log.Information($"downloading events from {Util.DateTimeToString(from)}");
                var result = Downloader.DownloadFromTill(latestDate, from);
                Log.Information($"downloaded {result.Count} new events");
                JsonImporter.ImportIncremential(db, result);
                Log.Information($"imported {result.Count} new events");
                latestDate = new DateTime(from.Year, from.Month, from.Day);
            }

            var newLatestDate = GetLatestEventDateTime();
            Log.Information($"updated database (latest event was on {Util.DateTimeToString(newLatestDate)}");
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
                return DateTime.Now;
        }
    }
}
