using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Team : IDisposable, INotifyBestLapChanged
    {
        private readonly INotifyBestLapChanged _carClass;
        private readonly TelemetryViewModel _telemetry;

        private Driver _activeDriver;
        private Lap _bestLap;
        private List<Lap> _bestNLaps;
        private Lap _currentLap;
        private ObservableCollection<Driver> _drivers;
        private ObservableCollection<Lap> _lastNLaps;
        private Lap _lastLap;
        private int _leaderboardPosition = -1;

        public Team(INotifyBestLapChanged carClass, TelemetryViewModel telemetry)
        {
            _carClass = carClass;
            _telemetry = telemetry;

            _carClass.BestLapChanged += OnCarClassBestLapChanged;
            _telemetry.PropertyChanged += OnTelemetryPropertyChanged;
        }

        public Driver ActiveDriver
        {
            get => _activeDriver;
            set
            {
                if (_activeDriver != value)
                {
                    _activeDriver = value;
                    OnActiveDriverChanged();
                }
            }
        }

        public Lap BestLap
        {
            get => _bestLap;
            set
            {
                if (_bestLap != value)
                {
                    _bestLap = value;
                    OnBestLapChanged();
                }
            }
        }

        public List<Lap> BestNLaps
        {
            get
            {
                _bestNLaps ??= new List<Lap>();

                return _bestNLaps;
            }
        }

        public TimeSpan? BestNLapsAverage
        {
            get
            {
                if (BestNLaps.Any() == false)
                {
                    return null;
                }
                else
                {
                    return TimeSpan.FromSeconds(BestNLaps.Average(x => x.Time.TotalSeconds));
                }
            }
        }

        public string BestNLapsColor
        {
            get
            {
                if (BestNLapsAverage > TimeSpan.Zero && DeltaToBestN == 0)
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

        public string BestLapColor { get; private set; }

        public string CarNumber { get; set; }

        public Lap CurrentLap
        {
            get => _currentLap;
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

        public double DeltaToBestN { get; set; }

        public double DeltaToPlayerBest { get; set; }

        public double DeltaToPlayerBestN { get; set; }

        public double DeltaToPlayerLast { get; set; }

        public double DeltaToPlayerLastN { get; set; }

        public ObservableCollection<Driver> Drivers
        {
            get
            {
                if (_drivers == null)
                {
                    _drivers = new ObservableCollection<Driver>();
                    _drivers.CollectionChanged += OnDriversCollectionChanged;
                }

                return _drivers;
            }
        }

        public double EstimatedDelta
        {
            get
            {
                TimeSpan? referenceLapTime;

                switch (_telemetry.ReferenceLap)
                {
                    case ReferenceLap.PersonalBest:
                        referenceLapTime = ActiveDriver?.BestLap?.Time;
                        break;
                    case ReferenceLap.TeamBest:
                        referenceLapTime = BestLap?.Time;
                        break;
                    case ReferenceLap.TeamBestN:
                        referenceLapTime = BestNLapsAverage;
                        break;
                    case ReferenceLap.TeamLast:
                        referenceLapTime = LastLap?.Time;
                        break;
                    case ReferenceLap.TeamLastN:
                        referenceLapTime = LastNLapsAverage;
                        break;
                    case ReferenceLap.ClassBest:
                        referenceLapTime = BestLap?.Time - TimeSpan.FromSeconds(DeltaToBest);
                        break;
                    default:
                        throw new InvalidEnumArgumentException(nameof(_telemetry.ReferenceLap), (int)_telemetry.ReferenceLap, typeof(ReferenceLap));
                }

                return (EstimatedLapTime - referenceLapTime)?.TotalSeconds ?? 0D;
            }
        }

        public string EstimatedLapColor
        {
            get
            {
                TimeSpan? referenceLapTime;

                switch (_telemetry.ReferenceLap)
                {
                    case ReferenceLap.PersonalBest:
                        referenceLapTime = ActiveDriver?.BestLap?.Time;
                        break;
                    case ReferenceLap.TeamBest:
                        referenceLapTime = BestLap?.Time;
                        break;
                    case ReferenceLap.TeamBestN:
                        referenceLapTime = BestNLapsAverage;
                        break;
                    case ReferenceLap.TeamLast:
                        referenceLapTime = LastLap?.Time;
                        break;
                    case ReferenceLap.TeamLastN:
                        referenceLapTime = LastNLapsAverage;
                        break;
                    case ReferenceLap.ClassBest:
                        referenceLapTime = BestLap?.Time - TimeSpan.FromSeconds(DeltaToBest);
                        break;
                    default:
                        throw new InvalidEnumArgumentException(nameof(_telemetry.ReferenceLap), (int)_telemetry.ReferenceLap, typeof(ReferenceLap));
                }

                if (EstimatedLapTime <= referenceLapTime)
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
                    return Colors.White;
                }
            }
        }

        public TimeSpan? EstimatedLapTime { get; set; }

        public double GapToClassLeader { get; set; }

        public string GapToClassLeaderString { get; set; }

        public double GapToLeader { get; set; }

        public string GapToLeaderString { get; set; }

        public double GapToPlayer { get; set; }

        public string GapToPlayerString { get; set; }

        public int GridPosition { get; set; } = -1;

        public int GridPositionInClass { get; set; } = -1;

        public double Interval { get; set; }

        public double IntervalInClass { get; set; }

        public string IntervalInClassString { get; set; }

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
                    return (int?)ActiveDriver?.IRating;
                }
            }
        }

        public bool IsConnected { get; set; }

        public bool IsDirty { get; set; }

        public bool IsInPit { get; set; }

        public bool IsPlayer { get; set; }

        public int LapsCompleted => (int)(CurrentLapHighPrecision ?? 0D);

        public ObservableCollection<Lap> LastNLaps
        {
            get
            {
                if (_lastNLaps == null)
                {
                    _lastNLaps = new ObservableCollection<Lap>();
                    _lastNLaps.CollectionChanged += OnLastNLapsCollectionChanged;
                }

                return _lastNLaps;
            }
        }

        public TimeSpan? LastNLapsAverage
        {
            get
            {
                if (LastNLaps.Any() == false)
                {
                    return null;
                }
                else
                {
                    return TimeSpan.FromSeconds(LastNLaps.Average(x => x.Time.TotalSeconds));
                }
            }
        }

        public string LastNLapsColor
        {
            get
            {
                if (LastNLapsAverage > TimeSpan.Zero && LastNLapsAverage == BestNLapsAverage)
                {
                    if (BestNLapsColor == Colors.Purple)
                    {
                        return Colors.Purple;
                    }
                    else
                    {
                        return Colors.Green;
                    }
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
            get => _lastLap;
            set
            {
                if (_lastLap != value)
                {
                    _lastLap = value;
                    OnLastLapChanged(ActiveDriver);
                }
            }
        }

        public string LastLapColor
        {
            get
            {
                if (LastLap?.Time > TimeSpan.Zero && LastLap.Time == BestLap?.Time)
                {
                    if (BestLapColor == Colors.Purple)
                    {
                        return Colors.Purple;
                    }
                    else
                    {
                        return Colors.Green;
                    }
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

        public int LeaderboardPosition
        {
            get => _leaderboardPosition;
            set
            {
                if (_leaderboardPosition != value)
                {
                    _leaderboardPosition = value;
                    OnLeaderboardPositionChanged();
                }
            }
        }

        public int LivePosition { get; set; } = -1;

        public int LivePositionInClass { get; set; } = -1;

        public string Name { get; set; }

        public int PositionsGained => GridPosition == -1 ? 0 : GridPosition - LivePosition;

        public int PositionsGainedInClass => GridPositionInClass == -1 ? 0 : GridPositionInClass - LivePositionInClass;

        public double? RelativeGapToPlayer { get; set; }

        public string RelativeGapToPlayerColor
        {
            get
            {
                if (CurrentLapHighPrecision > 0)
                {
                    if (GapToPlayer < 0 && (GapToPlayerString?.EndsWith("L") == true || RelativeGapToPlayer >= 0))
                    {
                        return Colors.Orange;
                    }
                    else if (GapToPlayer > 0 && (GapToPlayerString?.EndsWith("L") == true || RelativeGapToPlayer <= 0))
                    {
                        return Colors.Blue;
                    }
                }

                return Colors.White;
            }
        }

        public string RelativeGapToPlayerString => _telemetry.EnableInverseGapStrings == true ? $"{RelativeGapToPlayer:-0.0;+0.0}" : $"{RelativeGapToPlayer:+0.0;-0.0}";

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_carClass != null)
                {
                    _carClass.BestLapChanged -= OnCarClassBestLapChanged;
                }

                if (_drivers != null)
                {
                    _drivers.CollectionChanged -= OnDriversCollectionChanged;
                }

                if (_lastNLaps != null)
                {
                    _lastNLaps.CollectionChanged -= OnLastNLapsCollectionChanged;
                }

                if (_telemetry != null)
                {
                    _telemetry.PropertyChanged -= OnTelemetryPropertyChanged;
                }
            }
        }

        private void OnActiveDriverChanged()
        {
            IsDirty = true;
        }

        private void OnBestLapChanged()
        {
            BestLapChanged?.Invoke(this, new BestLapChangedEventArgs(BestLap));
        }

        private void OnCarClassBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (BestLap?.Time > TimeSpan.Zero && BestLap.Time == e.Lap?.Time)
            {
                BestLapColor = Colors.Purple;
            }
            else if (IsPlayer == true)
            {
                BestLapColor = Colors.Yellow;
            }
            else
            {
                BestLapColor = Colors.White;
            }
        }

        private void OnCurrentLapChanged()
        {
            if (LapsCompleted != Drivers.Sum(x => x.LapsCompleted) && ActiveDriver != null)
            {
                ActiveDriver.LapsCompleted = LapsCompleted - Drivers.Where(x => x != ActiveDriver).Sum(x => x.LapsCompleted);
            }
        }

        private void OnCurrentLapChanging()
        {
            if (CurrentLap?.Number > 0 && CurrentLap?.Time > TimeSpan.Zero)
            {
                LastLap = CurrentLap;
            }
        }

        private void OnDriverBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (e.Lap == null || e.Lap.Time < (BestLap?.Time ?? TimeSpan.MaxValue))
            {
                BestLap = e.Lap;
            }
        }

        private void OnDriversCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (Driver driver in e.OldItems)
                {
                    driver.BestLapChanged -= OnDriverBestLapChanged;
                }
            }

            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (Driver driver in e.NewItems)
                {
                    driver.BestLapChanged += OnDriverBestLapChanged;
                }
            }
        }

        private void OnLastLapChanged(Driver activeDriver)
        {
            if (LastLap.IsInLap == false && LastLap.IsOutLap == false && LastLap.IsDirty == false && LastLap.Number > 1 && LastLap.Time < (activeDriver?.BestLap?.Time ?? TimeSpan.MaxValue))
            {
                activeDriver.BestLap = LastLap;
            }

            if (LastLap.Number > 0)
            {
                if (LastNLaps.Count() == _telemetry.NLaps)
                {
                    LastNLaps.RemoveAt(_telemetry.NLaps - 1);
                }

                LastNLaps.Insert(0, LastLap);
            }
        }

        private void OnLastNLapsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (BestNLaps.Count < LastNLaps.Count(x => x.IsInLap == false && x.IsOutLap == false && x.IsDirty == false && x.Number > 1) || LastNLapsAverage < BestNLapsAverage)
            {
                BestNLaps.Clear();

                BestNLaps.AddRange(LastNLaps.Where(x => x.IsInLap == false && x.IsOutLap == false && x.IsDirty == false && x.Number > 1));
            }
        }

        private void OnLeaderboardPositionChanged()
        {
            IsDirty = true;
        }

        private void OnTelemetryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TelemetryViewModel.NLaps))
            {
                if (BestNLaps.Count() > _telemetry.NLaps)
                {
                    foreach (var lap in BestNLaps.Skip(_telemetry.NLaps).ToList())
                    {
                        BestNLaps.Remove(lap);
                    }
                }

                if (LastNLaps.Count() > _telemetry.NLaps)
                {
                    foreach (var lap in LastNLaps.Skip(_telemetry.NLaps).ToList())
                    {
                        LastNLaps.Remove(lap);
                    }
                }
            }
        }

        #region Interface: IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Interface: INotifyBestLapChanged
        public event EventHandler<BestLapChangedEventArgs> BestLapChanged;
        #endregion
    }
}
