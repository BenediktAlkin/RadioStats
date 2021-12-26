﻿using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class Statistics
    {
        public static int UniqueSongCount(DateTime from, DateTime to)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();
            return db.Events
                .Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix)
                .Select(e => e.SongId)
                .Distinct()
                .Count();
        }
        public static int TotalSongCount(DateTime from, DateTime to)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();
            return db.Events
                .Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix)
                .Count();
        }
        public static int TotalSongMinutes(DateTime from, DateTime to) => (int)(TotalSongSeconds(from, to) / 60);
        public static int TotalSongSeconds(DateTime from, DateTime to)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();
            return db.Events
                .Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix)
                .Select(e => e.Duration)
                .Sum();
        }

        public static byte[] SongVarietyByHourPlot(DateTime from, DateTime to)
        {
            var varietyByHour = SongVarietyByHour(from, to);
            var plt = new Plot(600, 400);
            var labels = varietyByHour.Select(vbh => vbh.Item1.ToString("HH:mm")).ToArray();
            // take every other element
            labels = labels.Select((l, i) => i % 3 == 0 ? l : string.Empty).ToArray();
            plt.XTicks(labels);
            var values = varietyByHour.Select(vbh => vbh.Item2).ToArray();
            plt.AddScatter(
                Enumerable.Range(0, varietyByHour.Count).Select(i => (double)i).ToArray(), 
                values);
            plt.Title("Musikvielfalt per Stunde");
            // set yrange to 0.5, 1.5
            var ymin = Math.Min(0.5, values.Min());
            var ymax = Math.Max(1.5, values.Max());
            plt.SetAxisLimitsY(ymin, ymax);
            return plt.GetImageBytes();
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
            var eventsInTimeSpan = db.Events.Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix).ToList();
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
            }
            var averageFrequencySum = frequencySumByHour.Average(fsbh => fsbh.FrequencySum);
            static double GetVariety(int frequencySum, double averageFrequencySum)
            {
                if (frequencySum == 0 || averageFrequencySum == 0) return 1;
                // TODO for some reason sometimes at 00:00 this is very high
                return Math.Clamp(frequencySum / averageFrequencySum, 0, 2);
            }
            return frequencySumByHour
                .Select(fsbh => (fsbh.Hour, GetVariety(fsbh.FrequencySum, averageFrequencySum)))
                .OrderBy(hourVariety => hourVariety.Hour)
                .ToList();
        }

        public static List<(Song, int)> MostPlayedSongs(DateTime from, DateTime to, int count)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();

            // get counts
            var eventsInTimeSpan = db.Events.Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix).ToList();
            var counts = eventsInTimeSpan.GroupBy(e => e.SongId);

            // get song objects of topK songs
            var topKCounts = counts
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => new { SongId = g.Key, Count = g.Count() })
                .ToList();
            var songs = db.Songs
                .Where(s => topKCounts.Select(songCount => songCount.SongId).Contains(s.Id))
                .Include(s => s.Artists)
                .ToList();

            // match song to count
            return topKCounts.Select(songCount => (songs.First(s => s.Id == songCount.SongId), songCount.Count)).ToList();
        }

        public static byte[] AverageDailySongVarietyByHourPlot(DateTime from, DateTime to)
        {
            var varietyByHour = AverageDailySongVarietyByHour(from, to);
            var plt = new Plot(600, 200);
            var labels = varietyByHour.Select(vbh => vbh.Item1.ToString("HH:mm")).ToArray();
            // take every other element
            labels = labels.Select((l, i) => i % 3 == 0 ? l : string.Empty).ToArray();
            plt.XTicks(labels);
            var values = varietyByHour.Select(vbh => vbh.Item2).ToArray();
            var stds = varietyByHour.Select(vbh => vbh.Item3).ToArray();
            plt.AddScatter(
                Enumerable.Range(0, varietyByHour.Count).Select(i => (double)i).ToArray(),
                values);
            plt.AddFill(
                Enumerable.Range(0, varietyByHour.Count).Select(i => (double)i).ToArray(),
                varietyByHour.Select(vbh => vbh.Item2 - vbh.Item3).ToArray(),
                varietyByHour.Select(vbh => vbh.Item2 + vbh.Item3).ToArray());
            plt.Title($"Durchschnittliche Musikvielfalt per Stunde {from.Year}");
            // set yrange to 0.5, 1.5
            var ymin = Math.Min(0.5, values.Min());
            var ymax = Math.Max(1.5, values.Max());
            plt.SetAxisLimitsY(ymin, ymax);
            return plt.GetImageBytes();
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
