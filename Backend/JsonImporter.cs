﻿using Backend.Entities;
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
        public static void ImportJsonEvents(List<JsonEvent> events)
        {
            if (events == null) return;

            UnifyJsonEvents(events);
            ImportEventsToDb(events);
        }


        private static void UnifyJsonEvents(List<JsonEvent> jsonEvents)
        {
            Log.Information($"unifying {jsonEvents.Count} JsonEvents");
            foreach (var jsonEvent in jsonEvents)
            {
                var artistsString = jsonEvent.Artist;
                var wasChanged = false;
                // replace all FEAT_VARIATIONS with FEAT_STRING
                foreach (var key in Constants.FEAT_VARIATIONS)
                {
                    if (artistsString.Contains(key))
                    {
                        artistsString = artistsString.Replace(key, Constants.FEAT_STRING);
                        wasChanged = true;
                    }
                }
                if (wasChanged)
                {
                    Log.Information($"changed ArtistsString from \"{jsonEvent.Artist}\" to \"{artistsString}\"");
                    jsonEvent.Artist = artistsString;
                }
            }
        }


        private static void ImportEventsToDb(List<JsonEvent> jsonEvents)
        {
            using var db = new DatabaseContext();


            foreach(var jsonEvent in jsonEvents)
            {
                // extract multiple ArtistNames from single ArtistsString
                List<string> artistStrings;
                if (jsonEvent.Artist.Contains(Constants.FEAT_STRING))
                    artistStrings = jsonEvent.Artist
                        .Split(new string[] { Constants.FEAT_STRING }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                else
                    artistStrings = new List<string> { jsonEvent.Artist };
                
                // insert new Artists and/or find existing ones
                var dbArtists = new List<Artist>();
                foreach(var artistName in artistStrings)
                {
                    var dbArtist = db.Artists.FirstOrDefault(a => a.Name == artistName);
                    if (dbArtist == null)
                    {
                        dbArtist = new Artist { Name = artistName };
                        db.Artists.Add(dbArtist);
                        db.SaveChanges();
                        Log.Information($"inserted artist \"{artistName}\"");
                    }
                    dbArtists.Add(dbArtist);
                }
                var primaryArtist = dbArtists[0];

                // insert new Song or find existing one
                var dbSong = db.Songs.FirstOrDefault(s => s.Name == jsonEvent.SongName && s.PrimaryArtistId == primaryArtist.Id);
                if (dbSong == null)
                {
                    dbSong = new Song()
                    {
                        Artists = dbArtists,
                        PrimaryArtist = primaryArtist,
                        Name = jsonEvent.SongName,
                    };
                    db.Songs.Add(dbSong);
                    db.SaveChanges();
                    Log.Information($"inserted song \"{dbSong}\"");
                }

                // insert event or find existing one
                var eventTimeUnix = Util.UnixTimestamp(jsonEvent.Time);
                var dbEvent = db.Events.FirstOrDefault(e => e.StartTimeUnix == eventTimeUnix);
                if(dbEvent == null)
                {
                    dbEvent = new Event()
                    {
                        Duration = jsonEvent.Length,
                        Song = dbSong,
                        StartTimeUnix = eventTimeUnix,
                    };
                    db.Events.Add(dbEvent);
                    db.SaveChanges();
                    Log.Information($"inserted event {dbEvent}");
                }
            }
        }
    }
}
