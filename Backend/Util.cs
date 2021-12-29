using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{

    public static class Util
    {
        public static int UnixTimestamp(DateTime dateTime)
        {
            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static DateTime UnixTimestampToDateTime(int dateTime)
        {
            return new DateTime(1970, 1, 1).AddSeconds(dateTime);
        }
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
    }
}
