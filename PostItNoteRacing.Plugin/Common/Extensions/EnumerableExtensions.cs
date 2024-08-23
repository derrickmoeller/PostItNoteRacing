using System;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static double StDev(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
    }
}
