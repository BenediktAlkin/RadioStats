using Backend.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class JsonImporter
    {
        public static void ImportIncremential(DatabaseContext db, List<JsonEvent> events)
        {
            if (events.Count == 0) return;

            UnifyJsonEvents(events);
            ImportEventsToDb(db, events);
        }


        private static void UnifyJsonEvents(List<JsonEvent> list)
        {
            Log.Information("Unifying JsonArtists");
            for (var i = 0; i < list.Count; i++)
            {
                var artist = list[i].Artist;
                foreach (var key in Constants.FEAT_VARIATIONS.Keys)
                {
                    if (artist.Contains(key))
                        artist = artist.Replace(key, Constants.FEAT_VARIATIONS[key]);
                }
                list[i].Artist = artist;
                if ((i + 1) % 100 == 0)
                    Log.Information($"Unified {i}/{list.Count} Artists");
            }
            Log.Information("Finished Unifying JsonArtists");
        }

        private static void ImportEventsToDb(DatabaseContext db, List<JsonEvent> events)
        {
            // TODO this method can be made much more efficient
            var artists = new Dictionary<string, Artist>();
            var songs = new List<Song>();
            var evts = new List<Event>();

            Log.Information("prepare insert artists");
            foreach(var group in events.GroupBy(e => e.Artist).ToList())
            {
                var artist = new Artist()
                {
                    Name = group.Key,
                    Songs = new List<Song>()
                };

                if (artist.CouldBeMultipleArtists)
                {
                    foreach (var subArtist in artist.ExtractArtists())
                    {
                        if (!artists.ContainsKey(subArtist.Name))
                            artists[subArtist.Name] = subArtist;
                    }
                }
                else
                    artists[group.Key] = artist;
            }


            Log.Information("prepare insert songs");
            foreach(var group in events.GroupBy(e => new { e.Artist, e.SongName }).ToList())
            {
                var list = new List<Artist>();
                var tempArtist = new Artist() { Name = group.Key.Artist };
                if (tempArtist.CouldBeMultipleArtists)
                {
                    foreach (var subArtist in tempArtist.ExtractArtists())
                        list.Add(artists[subArtist.Name]);
                }
                else
                    list.Add(artists[group.Key.Artist]);

                var song = new Song()
                {
                    Artists = list,
                    PrimaryArtist = list.First(),
                    Name = group.Key.SongName,
                    Events = new List<Event>()
                };

                songs.Add(song);
                foreach(var artist in song.Artists)
                    artist.Songs.Add(song);
            }


            Log.Information($"prepare insert events (count={events.Count})");
            events = events.OrderBy(e => e.Time).ToList();
            Event prevEvt = null;
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                var evtSongs = songs.First(s => s.Name == e.SongName && s.ArtistsString == e.Artist);
                var evt = new Event()
                {
                    Duration = e.Length,
                    Song = evtSongs,
                    StartTimeUnix = Util.UnixTimestamp(e.Time),
                };
                if (prevEvt != null && prevEvt.StartTimeUnix == evt.StartTimeUnix)
                    continue;
                evt.Song.Events.Add(evt);
                evts.Add(evt);

                prevEvt = evt;
            }

            Log.Information($"saving changes");
            var artistlist = artists.Values.ToList();
            db.Artists.AddRange(artistlist);
            db.Songs.AddRange(songs);
            db.Events.AddRange(evts);
            db.SaveChanges();
            Log.Information("finished db update");
        }
    }
}
