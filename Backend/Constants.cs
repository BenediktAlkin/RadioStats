using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class Constants
    {
        public const string FEAT_STRING = " & ";
        public static readonly List<string> FEAT_VARIATIONS = new()
        {
            " feat ",
            " Feat ",
            " Feat. ",
            " feat. ",
            " . ",
        };
    }
}
