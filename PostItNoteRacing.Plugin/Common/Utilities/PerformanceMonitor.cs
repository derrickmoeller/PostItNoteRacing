using PostItNoteRacing.Common.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

namespace PostItNoteRacing.Common.Utilities
{
    public class PerformanceMonitor(int maxObservations = 1000)
    {
        private readonly FixedSizeObservableCollection<double> _observations = new (maxObservations);
        private readonly Stopwatch _stopwatch = new ();

        public double AvgObservation => _observations.Average();

        public double MaxObservation => _observations.Max();

        public double MinObservation => _observations.Min();

        public double StDevObservation => _observations.StDev();

        public void Time(Action action)
        {
            _stopwatch.Restart();

            action();

            _stopwatch.Stop();

            _observations.Add(_stopwatch.Elapsed.TotalMilliseconds);
        }

        public override string ToString()
        {
            return $"Avg: {AvgObservation}, Min: {MinObservation}, Max: {MaxObservation}, StDev: {StDevObservation}";
        }
    }
}
