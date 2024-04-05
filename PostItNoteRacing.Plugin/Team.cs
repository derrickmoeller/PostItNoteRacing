using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PostItNoteRacing.Plugin
{
    internal class Team : IDisposable
    {
        private int _lapsCompleted = 0;
        private ObservableCollection<TimeSpan> _lastFiveLaps;
        private TimeSpan _lastLapTime;

        private List<TimeSpan> BestFiveLaps { get; } = new List<TimeSpan>();

        private ObservableCollection<TimeSpan> LastFiveLaps
        {
            get
            {
                if (_lastFiveLaps == null)
                {
                    _lastFiveLaps = new ObservableCollection<TimeSpan>();
                    _lastFiveLaps.CollectionChanged += OnLastFiveLapsCollectionChanged;
                }

                return _lastFiveLaps;
            }
        }

        public TimeSpan BestFiveLapsAverage
        {
            get
            {
                if (BestFiveLaps.Any() == false)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return TimeSpan.FromSeconds(BestFiveLaps.Average(x => x.TotalSeconds));
                }
            }
        }

        public string BestFiveLapsColor
        {
            get
            {
                if (DeltaToBestFive == 0)
                {
                    return Colors.Purple;
                }
                else if (IsPlayer == true)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public string BestLapColor
        {
            get
            {
                if (DeltaToBest == 0)
                {
                    return Colors.Purple;
                }
                else if (IsPlayer == true)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public TimeSpan BestLapTime { get; set; }

        public string CarNumber { get; set; }

        public double? CurrentLapHighPrecision { get; set; }

        public TimeSpan? CurrentLapTime { get; set; }

        public double? DeltaToBest { get; set; }

        public double? DeltaToBestFive { get; set; }

        public double? DeltaToPlayerBest { get; set; }

        public double? DeltaToPlayerBestFive { get; set; }

        public double? DeltaToPlayerLast { get; set; }

        public double? DeltaToPlayerLastFive { get; set; }

        public List<Driver> Drivers { get; } = new List<Driver>();

        public double? GapToLeader { get; set; }

        public string GapToLeaderString { get; set; }

        public double? GapToPlayer { get; set; }

        public string GapToPlayerString { get; set; }

        public double? Interval { get; set; }

        public string IntervalString { get; set; }

        public int? IRating
        {
            get
            {
                var filteredDrivers = Drivers.Where(x => x.IRating > 0 && x.LapsCompleted > 0);

                if (filteredDrivers.Count() > 0)
                {
                    return (int)Math.Round(filteredDrivers.Sum(x => x.IRating.Value * x.LapsCompleted) / filteredDrivers.Sum(x => x.LapsCompleted));
                }
                else
                {
                    return (int?)Drivers.SingleOrDefault(x => x.IsActive == true)?.IRating;
                }
            }
        }
        
        public bool? IsConnected { get; set; }

        public bool? IsInPit { get; set; }

        public bool? IsPlayer { get; set; }

        public int LapsCompleted
        {
            get { return _lapsCompleted; }
            set
            {
                if (_lapsCompleted != value)
                {
                    _lapsCompleted = value;
                    OnLapsCompletedChanged();
                }
            }
        }

        public TimeSpan LastFiveLapsAverage
        {
            get
            {
                if (LastFiveLaps.Any() == false)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return TimeSpan.FromSeconds(LastFiveLaps.Average(x => x.TotalSeconds));
                }
            }
        }

        public string LastFiveLapsColor
        {
            get
            {
                if (LastFiveLapsAverage != TimeSpan.Zero && LastFiveLapsAverage == BestFiveLapsAverage)
                {
                    return Colors.Green;
                }
                else if (IsPlayer == true)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public string LastLapColor
        {
            get
            {
                if (LastLapTime != TimeSpan.Zero && LastLapTime == BestLapTime)
                {
                    return Colors.Green;
                }
                else if (IsPlayer == true)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public TimeSpan LastLapTime
        {
            get { return _lastLapTime; }
            set
            {
                if (_lastLapTime != value)
                {
                    _lastLapTime = value;
                    OnLastLapTimeChanged();
                }
            }
        }

        public int LeaderboardPosition { get; set; } = -1;

        public int LivePosition { get; set; } = -1;

        public int LivePositionInClass { get; set; } = -1;

        public string Name { get; set; }

        public double? RelativeGapToPlayer { get; set; }

        public string RelativeGapToPlayerColor
        {
            get
            {
                if (GapToPlayer < 0 && (GapToPlayerString?.EndsWith("L") == true || RelativeGapToPlayer >= 0))
                {
                    return Colors.Orange;
                }
                else if (GapToPlayer > 0 && (GapToPlayerString?.EndsWith("L") == true || RelativeGapToPlayer <= 0))
                {
                    return Colors.Blue;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public string RelativeGapToPlayerString => $"{RelativeGapToPlayer:-0.0;+0.0}";

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lastFiveLaps != null)
                {
                    _lastFiveLaps.CollectionChanged -= OnLastFiveLapsCollectionChanged;
                }
            }
        }

        private void OnLapsCompletedChanged()
        {
            if (LapsCompleted != Drivers.Sum(x => x.LapsCompleted))
            {
                var driver = Drivers.SingleOrDefault(x => x.IsActive == true);
                if (driver != null)
                {
                    driver.LapsCompleted = LapsCompleted - Drivers.Where(x => x.IsActive == false).Sum(x => x.LapsCompleted);
                }
            }
        }

        private void OnLastFiveLapsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (BestFiveLaps.Count < 5 || LastFiveLapsAverage < BestFiveLapsAverage)
            {
                BestFiveLaps.Clear();

                BestFiveLaps.AddRange(LastFiveLaps);
            }
        }

        private void OnLastLapTimeChanged()
        {
            if (LastFiveLaps.Count == 5)
            {
                LastFiveLaps.RemoveAt(4);
            }

            LastFiveLaps.Insert(0, LastLapTime);
        }

        #region Interface: IDispose
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
