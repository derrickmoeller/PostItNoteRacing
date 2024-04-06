using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Team : IDisposable
    {
        private Lap _bestLap;
        private Lap _currentLap;
        private ObservableCollection<TimeSpan> _lastFiveLaps;
        private Lap _lastLap;

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

        public TimeSpan? BestFiveLapsAverage
        {
            get
            {
                if (BestFiveLaps.Any() == false)
                {
                    return null;
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
                if (BestFiveLapsAverage > TimeSpan.Zero && DeltaToBestFive == 0)
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

        public Lap BestLap
        {
            get { return _bestLap; }
            set
            {
                if (_bestLap != value)
                {
                    _bestLap = value;
                    OnBestLapChanged();
                }
            }
        }

        public string BestLapColor
        {
            get
            {
                if (BestLapTime > TimeSpan.Zero && DeltaToBest == 0)
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

        public TimeSpan? BestLapTime { get; set; }

        public string CarNumber { get; set; }

        public Lap CurrentLap
        {
            get { return _currentLap; }
            set
            {
                if (_currentLap != value)
                {
                    OnCurrentLapChanging();
                    _currentLap = value;
                    OnCurrentLapChanged();
                }
            }
        }

        public double? CurrentLapHighPrecision { get; set; }

        public double DeltaToBest { get; set; }

        public double DeltaToBestFive { get; set; }

        public double DeltaToPlayerBest { get; set; }

        public double DeltaToPlayerBestFive { get; set; }

        public double DeltaToPlayerLast { get; set; }

        public double DeltaToPlayerLastFive { get; set; }

        public List<Driver> Drivers { get; } = new List<Driver>();

        public double EstimatedDelta => (EstimatedLapTime - BestLap?.Time)?.TotalSeconds ?? 0D;

        public string EstimatedLapColor
        {
            get
            {
                if (EstimatedLapTime <= BestLap?.Time)
                {
                    if (DeltaToBest + EstimatedDelta < 0)
                    {
                        return Colors.Purple;
                    }
                    else
                    {
                        return Colors.Green;
                    }
                }
                else if (EstimatedLapTime != null)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.Gray;
                }
            }
        }

        public TimeSpan? EstimatedLapTime
        {
            get
            {
                if (BestLap != null && CurrentLap.MiniSectors.Any())
                {
                    var miniSector = CurrentLap.MiniSectors.OrderByDescending(x => x.TrackPosition).First();
                    var nextSector = BestLap.MiniSectors.OrderBy(x => x.TrackPosition).FirstOrDefault(x => x.TrackPosition >= miniSector.TrackPosition) ?? new MiniSector { Time = BestLap.Time, TrackPosition = 1 };
                    var lastSector = BestLap.MiniSectors.OrderByDescending(x => x.TrackPosition).FirstOrDefault(x => x.TrackPosition <= miniSector.TrackPosition) ?? new MiniSector { Time = TimeSpan.Zero, TrackPosition = 0 };

                    var interpolatedValue = GetLinearInterpolation(miniSector.TrackPosition, lastSector.TrackPosition, nextSector.TrackPosition, lastSector.Time.Ticks, nextSector.Time.Ticks);

                    return BestLap.Time + (miniSector.Time - TimeSpan.FromTicks(interpolatedValue));

                    long GetLinearInterpolation(double x, double x0, double x1, long y0, long y1)
                    {
                        if ((x1 - x0) == 0)
                        {
                            return (y0 + y1) / 2;
                        }

                        return (long)(y0 + (x - x0) * (y1 - y0) / (x1 - x0));
                    }
                }
                else
                {
                    return null;
                }
            }
        }

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

        public int LapsCompleted => (int)(CurrentLapHighPrecision ?? 0D);

        public TimeSpan? LastFiveLapsAverage
        {
            get
            {
                if (LastFiveLaps.Any() == false)
                {
                    return null;
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
                if (LastFiveLapsAverage > TimeSpan.Zero && LastFiveLapsAverage == BestFiveLapsAverage)
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

        public Lap LastLap
        {
            get { return _lastLap; }
            set
            {
                if (_lastLap != value)
                {
                    _lastLap = value;
                    OnLastLapChanged();
                }
            }
        }

        public string LastLapColor
        {
            get
            {
                if (LastLap?.Time > TimeSpan.Zero && LastLap.Time == BestLapTime)
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

        private void OnBestLapChanged()
        {
            if (BestLap?.Time < (BestLapTime ?? TimeSpan.MaxValue))
            {
                BestLapTime = BestLap.Time;
            }
        }

        private void OnCurrentLapChanged()
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

        private void OnCurrentLapChanging()
        {
            LastLap = CurrentLap;
        }

        private void OnLastFiveLapsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (BestFiveLaps.Count < 5 || LastFiveLapsAverage < BestFiveLapsAverage)
            {
                BestFiveLaps.Clear();

                BestFiveLaps.AddRange(LastFiveLaps);
            }
        }

        private void OnLastLapChanged()
        {
            if (LastLap.IsInLap == false && LastLap.IsOutLap == false && LastLap.Number > 1 && LastLap.Time < (BestLap?.Time ?? TimeSpan.MaxValue))
            {
                BestLap = LastLap;
            }

            if (LastLap.Number > 0)
            {
                if (LastFiveLaps.Count == 5)
                {
                    LastFiveLaps.RemoveAt(4);
                }

                LastFiveLaps.Insert(0, LastLap.Time);
            }
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
