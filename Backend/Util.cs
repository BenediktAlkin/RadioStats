using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{

    public static class Util
    {
        private static readonly string Dateformat = "yyyy-MM-dd";
        private static readonly string Timeformat = "HH:mm:ss";
        private static readonly string DateTimeFormat = $"{Dateformat} {Timeformat}";
        private static readonly string DateTimeFormatJson = $"{Dateformat}T{Timeformat}";

        public static string DateTimeToJsonString(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormatJson);
        }
        public static string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormat);
        }
        public static string DateTimeToDateString(DateTime datetime)
        {
            return datetime.ToString(Dateformat);
        }

        public static DateTime DateTimeFromString(string dateTime)
        {
            return DateTime.ParseExact(dateTime, DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
        }
        public static DateTime DateTimeFromJsonString(string dateTime)
        {
            return DateTime.ParseExact(dateTime, DateTimeFormatJson, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static int UnixTimestamp(DateTime dateTime)
        {
            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static DateTime UnixTimestampToDateTime(int dateTime)
        {
            return new DateTime(1970, 1, 1).AddSeconds(dateTime);
        }

        //public static DbQuery<Song> IncludeAll(DbSet<Song> songs)
        //{
        //    return songs.Include(Song.FK.Artists.ToString()).Include(Song.FK.Events.ToString());
        //}
        //public static DbQuery<Song> IncludeArtists(DbSet<Song> songs)
        //{
        //    return songs.Include(Song.FK.Artists.ToString());
        //}
        //public static DbQuery<Song> IncludeEvents(DbSet<Song> songs)
        //{
        //    return songs.Include(Song.FK.Events.ToString());
        //}


        //public static DbQuery<Artist> IncludeSongs(DbSet<Artist> artists)
        //{
        //    return artists.Include(Artist.FK.Songs.ToString());
        //}
        //public static DbQuery<Artist> IncludeAll(DbSet<Artist> artists)
        //{
        //    return artists.Include(Artist.FK.Songs.ToString());
        //}
        //public static DbQuery<Event> IncludeAll(DbSet<Event> events)
        //{
        //    return events.Include(Event.FK.Song.ToString()).Include($"{Event.FK.Song.ToString()}.{Song.FK.Artists.ToString()}");
        //}
        //public static bool IsSimilarTo(string str, string other, double similarityThreshold)
        //{
        //    var strchar = str.ToCharArray();
        //    var otherchar = other.ToCharArray();

        //    var i = 0;
        //    var j = 0;
        //    var equal = 0;
        //    while (i < strchar.Length && j < otherchar.Length)
        //    {
        //        if (strchar[i] == otherchar[j])
        //        {
        //            i++;
        //            j++;
        //            equal++;
        //        }
        //        else
        //        {
        //            if (j + 1 < otherchar.Length && strchar[i] == otherchar[j + 1])
        //                j++;
        //            if (i + 1 < strchar.Length && strchar[i + 1] == otherchar[j])
        //                i++;
        //        }
        //    }
        //    var minEqual = str.Length - (int)(str.Length / similarityThreshold);
        //    return equal >= minEqual;
        //}
    }
}
