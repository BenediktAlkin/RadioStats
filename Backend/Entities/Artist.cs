using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities
{
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Song> Songs { get; set; }
        public List<Song> PrimaryArtistSongs { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not Artist other)
                return false;

            if (other == null)
                return false;
            if (other.Name == Name)
                return true;
            return false;
        }

        public override string ToString() => $"{Name}";

        public bool CouldBeMultipleArtists => Name.Contains('&') || Name.ToLower().Contains("feat.");
        public List<Artist> ExtractArtists() 
        { 
            if (!CouldBeMultipleArtists) return new List<Artist>() { this };

            var list = Name.Split(new string[] { " & " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return list.Select(s => new Artist() { Name = s, Songs = new List<Song>() }).ToList();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
