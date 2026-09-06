using System.Diagnostics;

namespace TrackpadCapture
{
    internal static class MonotonicTimestamp
    {
        public static long NowNs()
        {
            return (long)(Stopwatch.GetTimestamp() * (1000000000.0 / Stopwatch.Frequency));
        }
    }
}
