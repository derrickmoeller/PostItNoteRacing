using System;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static double StDev(this IEnumerable<double> source)
        {
            double avg = source.Average();
            return Math.Sqrt(source.Average(v => Math.Pow(v - avg, 2)));
        }
    }
}
