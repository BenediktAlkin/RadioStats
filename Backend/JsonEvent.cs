using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{

    public class JsonEvent
    {
        public string SongName { get; set; }
        public string Artist { get; set; }
        public DateTime Time { get; set; }
        public int Length { get; set; }

        public override string ToString() => $"{Time} {SongName} - {Artist}";
    }
}
