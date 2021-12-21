using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class Statistics
    {
        public static List<ObjectCount<Song>> GetMostPlayedSongs(DateTime from, DateTime to, int count)
        {
            var fromUnix = Util.UnixTimestamp(from);
            var toUnix = Util.UnixTimestamp(to);

            using var db = new DatabaseContext();

            // get counts
            var eventsInTimeSpan = db.Events.Where(e => fromUnix < e.StartTimeUnix && e.StartTimeUnix < toUnix).ToList();
            var counts = eventsInTimeSpan.GroupBy(e => e.SongId).Select(g => new ObjectCount<int>(g.Key, g.Count())).ToList();

            // get song objects of topK songs
            var topKCounts = counts.OrderByDescending(idCount => idCount.Count).Take(count).ToList();
            var songs = db.Songs.Where(s => topKCounts.Select(idCount => idCount.Object).Contains(s.Id)).Include(s => s.Artists).ToList();

            // match song to count
            var songCounts = topKCounts.Select(idCount => new ObjectCount<Song>(songs.First(s => s.Id == idCount.Object), idCount.Count)).ToList();
            return songCounts.ToList();
        }


        public record ObjectCount<T>(T Object, int Count);
    }
}
