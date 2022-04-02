using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class Statistics
    {
        public class TimeSpanResult
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
        }

        #region MusicSeconds
        public class MusicDurationResult : TimeSpanResult
        {
            public int MusicSeconds { get; set; }
            public int MusicMinutes => MusicSeconds / 60;
            public int MusicHours => MusicMinutes / 60;
            public int Seconds => (int)(To - From).TotalSeconds;
            public int Minutes => (int)(To - From).TotalMinutes;
            public int Hours => (int)(To - From).TotalHours;
            public double MusicPercentage => (double)MusicSeconds / Math.Max(1, Seconds);
        }
        public static MusicDurationResult GetMusicDuration(DateTime from, DateTime to)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();
            var musicSeconds = db.Events
                .Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix)
                .Select(e => e.Duration)
                .Sum();

            return new MusicDurationResult
            {
                From = from,
                To = to,
                MusicSeconds = musicSeconds,
            };
        }
        public class AverageMusicDurationResult : MusicDurationResult
        {
            public int MusicSecondsMin { get; set; }
            public int MusicSecondsMax { get; set; }
            public double MusicPercentageMin => (double)MusicSecondsMin / Math.Max(1, Seconds);
            public double MusicPercentageMax => (double)MusicSecondsMax / Math.Max(1, Seconds);
        }
        public static List<AverageMusicDurationResult> GetAverageMusicDurationPerHour(DateTime from, DateTime to)
        {
            // retrieve MusicDurationResults for every hour
            var musicDurationResults = new List<MusicDurationResult>();
            var curFrom = new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second);
            while(curFrom < to)
            {
                var curTo = curFrom.AddHours(1);
                musicDurationResults.Add(GetMusicDuration(curFrom, curTo));
                curFrom = curTo;
            }
            
            // remove date information from MusicDurationResults
            foreach(var musicDurationResult in musicDurationResults)
            {
                musicDurationResult.From = Util.RemoveDateInfo(musicDurationResult.From);
                musicDurationResult.To = Util.RemoveDateInfo(musicDurationResult.To);
            }

            var dict = musicDurationResults.GroupBy(mdr => mdr.From).ToDictionary(g => g.Key, g => g.ToList());

            // average by hour
            // TODO this returns some weird values
            return musicDurationResults
                .GroupBy(mdr => mdr.From)
                .Select(g => new AverageMusicDurationResult
                {
                    From = g.Key,
                    To = g.First().To,
                    MusicSeconds = (int)Math.Round(g.Select(mdr => mdr.MusicSeconds).Average()),
                    MusicSecondsMax = g.Select(mdr => mdr.MusicSeconds).Max(),
                    MusicSecondsMin = g.Select(mdr => mdr.MusicSeconds).Min(),
                })
                .ToList();
        }
        
        public static byte[] GetAverageMusicDurationPerHourPlot(DateTime from, DateTime to)
        {
            var musicDurationByHour = GetAverageMusicDurationPerHour(from, to).OrderBy(amdr => amdr.From).ToList();
            var x = musicDurationByHour.Select(amdr => amdr.MusicPercentage * 100).ToArray();
            var xMin = musicDurationByHour.Select(amdr => amdr.MusicPercentageMin * 100).ToArray();
            var xMax = musicDurationByHour.Select(amdr => amdr.MusicPercentageMax * 100).ToArray();
            var xLabels = musicDurationByHour.Select(amdr => amdr.From.ToString("HH:mm")).ToArray();

            return Plotter.Instance.GetPlot(x: x, xLabels: xLabels, xMin: xMin, xMax: xMax);
        }
        #endregion

        #region UniqueSongs
        public class UniqueSongsResult : TimeSpanResult
        {
            public int SongCount { get; set; }
            public int UniqueSongCount { get; set; }
            public double UniqueSongPercentage => (double)UniqueSongCount / Math.Max(1, SongCount);
        }
        public static UniqueSongsResult GetUniqueSongs(DateTime from, DateTime to)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();
            var allSongs = db.Events
                .Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix)
                .Select(e => e.SongId)
                .ToList();
            var songCount = allSongs.Count;
            var uniqueSongCount = allSongs.Distinct().Count();

            return new UniqueSongsResult
            {
                From = from,
                To = to,
                SongCount = songCount,
                UniqueSongCount = uniqueSongCount,
            };
        }
        #endregion

        #region MostPlayedSongs
        public class MostPlayedSongsResult : TimeSpanResult
        {
            public int TopK { get; set; }
            public List<int> Counts { get; set; }
            public List<Song> Songs { get; set; }
        }
        public static MostPlayedSongsResult MostPlayedSongs(DateTime from, DateTime to, int count)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();

            // get counts
            var eventsInTimeSpan = db.Events.Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix).ToList();
            var counts = eventsInTimeSpan.GroupBy(e => e.SongId);

            // get song objects of topK songs
            var topKBySongId = counts
                .OrderByDescending(g => g.Count())
                .Take(count)
                .ToList();
            var songs = db.Songs
                .Where(s => topKBySongId.Select(group => group.Key).Contains(s.Id))
                .Include(s => s.Artists)
                .ToList();

            return new MostPlayedSongsResult
            {
                From = from,
                To = to,
                TopK = count,
                Songs = topKBySongId.Select(group => songs.First(s => s.Id == group.Key)).ToList(),
                Counts = topKBySongId.Select(group => group.Count()).ToList(),
            };
        }
        #endregion



        public static byte[] SongVarietyByHourPlot(DateTime from, DateTime to)
        {
            var varietyByHour = SongVarietyByHour(from, to);
            var values = varietyByHour.Select(vbh => vbh.Item2).ToArray();
            var xLabels = varietyByHour.Select(vbh => vbh.Item1.ToString("HH:mm")).ToArray();

            return Plotter.Instance.GetPlot(x: values, xLabels: xLabels);
        }
        public static List<(DateTime, double)> SongVarietyByHour(DateTime from, DateTime to)
        {
            // calculate frequency of song in timeframe
            // sum frequency of songs played in hour
            // divide by the average sum of all hours

            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();

            // get all hours in timespan (in case something goes wrong and an hour has no events)
            var allHours = new List<DateTime>();
            var curHour = new DateTime(from.Year, from.Month, from.Day, from.Hour, 0, 0);
            while (curHour < to)
            {
                allHours.Add(curHour);
                curHour += TimeSpan.FromHours(1);
            }

            // get counts
            var eventsInTimeSpan = db.Events
                .Include(e => e.Song).ThenInclude(s => s.Artists)
                .Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix)
                .ToList();
            var frequencyBySongId = eventsInTimeSpan.GroupBy(e => e.SongId).ToDictionary(g => g.Key, g => g.Count());

            // group events by hour
            var eventsByHour = eventsInTimeSpan.GroupBy(e => 
            {
                var dateTime = Util.UnixTimestampToDateTime(e.StartTimeUnix);
                var strippedDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
                return strippedDateTime;
            }).ToDictionary(g => g.Key, g => g.ToList());


            var frequencySumByHour = eventsByHour
                .Select(g => new { Hour = g.Key, FrequencySum = g.Value.Sum(e => frequencyBySongId[e.SongId]) })
                .ToList();
            // add 0 frequencies for hours without events (in case of error)
            var hoursWithEvents = eventsByHour.Keys.ToList();
            foreach(var hour in allHours)
            {
                if (!hoursWithEvents.Contains(hour))
                    frequencySumByHour.Add(new { Hour = hour, FrequencySum = 0 });
                if (!eventsByHour.ContainsKey(hour))
                    eventsByHour[hour] = new List<Event>();
            }

            static double GetVariety(int frequencySum, double nSongs)
            {
                if (frequencySum == 0 || nSongs == 0) return 1;
                return nSongs / frequencySum;
            }

            // log for analysis
            Log.Debug($"Frequencies from {from:g} to {to:g} (totalEvents={eventsInTimeSpan.Count}, uniqueEvents={frequencyBySongId.Keys.Count})");
            var frequencyCounts = frequencyBySongId.GroupBy(f => f.Value).ToDictionary(g => g.Key, g => g.Count()).OrderByDescending(g => g.Key);
            foreach (var kv in frequencyCounts)
                Log.Debug($"{kv.Value} Songs were played {kv.Key} times");
            Log.Debug("Frequencies from the whole day");
            var songsById = eventsInTimeSpan.GroupBy(e => e.SongId).ToDictionary(g => g.Key, g => g.First().Song);
            foreach (var kv in frequencyBySongId.OrderByDescending(kv => kv.Value))
            {
                var song = songsById[kv.Key];
                Log.Debug($"{kv.Value}x {song.Name} - {song.ArtistsString}");
            }
            Log.Debug($"Song Varieties for {from:g} to {to:g}");
            foreach (var fsbh in frequencySumByHour)
            {
                Log.Debug($"{fsbh.Hour:t}: events={eventsByHour[fsbh.Hour].Count} frequency={fsbh.FrequencySum}");
                foreach(var e in eventsByHour[fsbh.Hour])
                    Log.Debug($"frequency={frequencyBySongId[e.SongId]} {e.Song.Name} - {e.Song.ArtistsString} (id={e.SongId})");
            }

            return frequencySumByHour
                .Select(fsbh => (fsbh.Hour, GetVariety(fsbh.FrequencySum, eventsByHour[fsbh.Hour].Count)))
                .OrderBy(hourVariety => hourVariety.Hour)
                .ToList();
        }
        public static byte[] AverageDailySongVarietyByHourPlot(DateTime from, DateTime to)
        {
            var varietyByHour = AverageDailySongVarietyByHour(from, to);
            var xLabels = varietyByHour.Select(vbh => vbh.Item1.ToString("HH:mm")).ToArray();
            var values = varietyByHour.Select(vbh => vbh.Item2).ToArray();
            var stds = varietyByHour.Select(vbh => vbh.Item3).ToArray();
            var xMin = Enumerable.Range(0, values.Length).Select(i => values[i] - stds[i]).ToArray();
            var xMax = Enumerable.Range(0, values.Length).Select(i => values[i] + stds[i]).ToArray();
            return Plotter.Instance.GetPlot(x: values, xLabels: xLabels,
                xMin: xMin, xMax: xMax,
                width: 12, height: 4, title: $"Durchschnittliche Musikvielfalt per Stunde {from:yyyy}");
        }
        public static List<(DateTime, double, double)> AverageDailySongVarietyByHour(DateTime from, DateTime to)
        {
            var songVarieties = new Dictionary<int, List<double>>();
            var curFrom = new DateTime(from.Year, from.Month, from.Day);
            while (curFrom < to)
            {
                var curTo = curFrom + TimeSpan.FromDays(1);
                var curVarietyByHour = SongVarietyByHour(curFrom, curTo);
                foreach(var variety in curVarietyByHour)
                {
                    if (!songVarieties.ContainsKey(variety.Item1.Hour))
                        songVarieties[variety.Item1.Hour] = new List<double>();
                    songVarieties[variety.Item1.Hour].Add(variety.Item2);
                }
                curFrom += TimeSpan.FromDays(1);
            }
            return songVarieties
                .Select(sv => 
                (
                    new DateTime(1, 1, 1, sv.Key, 0, 0),
                    sv.Value.Average(), 
                    sv.Value.StandardDeviation())
                )
                .OrderBy(sv => sv.Item1)
                .ToList();
        }
    }
}
