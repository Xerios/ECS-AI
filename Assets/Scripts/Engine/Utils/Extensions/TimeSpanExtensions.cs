using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class TimeSpanExtensions
    {
        public static string FormatTimeSpan (this TimeSpan timeSpan)
        {
            Func<Tuple<int, string>, string> tupleFormatter = t => $"{t.Item1}{t.Item2}";
            var components = new List<Tuple<int, string> >
            {
                Tuple.Create((int)timeSpan.TotalDays, "d"),
                Tuple.Create(timeSpan.Hours, "h"),
                Tuple.Create(timeSpan.Minutes, "m"),
                Tuple.Create(timeSpan.Seconds, "s"),
            };

            components.RemoveAll(i => i.Item1 == 0);

            string extra = "";

            if (components.Count > 1) {
                var finalComponent = components[components.Count - 1];
                components.RemoveAt(components.Count - 1);
                extra = $" {tupleFormatter(finalComponent)}";
            }

            return $"{string.Join(", ", components.Select(tupleFormatter))}{extra}";
        }
    }
}