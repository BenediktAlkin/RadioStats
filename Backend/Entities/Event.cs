using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StartTimeUnix { get; set; }
        public int Duration { get; set; }

        public int SongId { get; set; }
        public Song Song { get; set; }

        public override string ToString()
        {
            var songString = "" + SongId;
            if (Song != null)
                songString = Song.Name;
            return $"{Util.UnixTimestampToDateTime(StartTimeUnix)} {songString} - {Duration}";
        }
    }
}
