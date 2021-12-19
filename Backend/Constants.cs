using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class Constants
    {
        public static readonly Dictionary<string, string> FEAT_VARIATIONS = new()
        {
            { " feat ", " & " },
            { " Feat ", " & " },
            { " Feat. ", " & " },
            { " feat. ", " & " },
            { " . ", " & " },
        };
    }
}
