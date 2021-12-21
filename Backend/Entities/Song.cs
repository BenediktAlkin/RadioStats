using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities
{
    public class Song
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Event> Events { get; set; }
        public List<Artist> Artists { get; set; }

        public int PrimaryArtistId { get; set; }
        public Artist PrimaryArtist { get; set; }



        public string ArtistsString => Artists?.Select(a => a.ToString()).Aggregate((a1, a2) => $"{a1} & {a2}");
        public override string ToString()
        {
            var artistString = Artists?.Select(a => a.ToString()).Aggregate((a1, a2) => $"{a1} & {a2}");
            return $"{Name} - {artistString}";
        }
    }
}