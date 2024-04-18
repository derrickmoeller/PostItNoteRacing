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
        private readonly SettingsViewModel _settings;

        private TimeSpan? _bestLapTime;
        private Lap _currentLap;
        private ObservableCollection<Driver> _drivers;
        private ObservableCollection<Lap> _lastNLaps;
        private Lap _lastLap;

        public Team(INotifyBestLapChanged carClass, SettingsViewModel settings)
        {
            _carClass = carClass;
            _settings = settings;

            _carClass.BestLapChanged += OnCarClassBestLapChanged;
            _settings.PropertyChanged += OnSettingsPropertyChanged;
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
                    return TimeSpan.FromSeconds(BestNLaps.Average(x => x.TotalSeconds));
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

        public TimeSpan? BestLapTime
        {
            get => _bestLapTime;
            set
            {
                if (_bestLapTime != value)
                {
                    _bestLapTime = value;
                    OnBestLapTimeChanged();
                }
            }
        }

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

        public double EstimatedDelta => (EstimatedLapTime - Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap?.Time)?.TotalSeconds ?? 0D;

        public string EstimatedLapColor
        {
            get
            {
                if (EstimatedLapTime <= Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap?.Time)
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

        public double GapToLeader { get; set; }

        public string GapToLeaderString { get; set; }

        public double GapToPlayer { get; set; }

        public string GapToPlayerString { get; set; }

        public double Interval { get; set; }

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

        public bool IsConnected { get; set; }

        public bool IsInPit { get; set; }

        public bool IsPlayer { get; set; }

        public int LapsCompleted => (int)(CurrentLapHighPrecision ?? 0D);

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
                    OnLastLapChanged(Drivers.SingleOrDefault(x => x.IsActive == true));
                }
            }
        }

        public string LastLapColor
        {
            get
            {
                if (LastLap?.Time > TimeSpan.Zero && LastLap.Time == BestLapTime)
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

        private List<TimeSpan> BestNLaps { get; } = new List<TimeSpan>();

        private ObservableCollection<Lap> LastNLaps
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

                if (_settings != null)
                {
                    _settings.PropertyChanged -= OnSettingsPropertyChanged;
                }
            }
        }

        private void OnBestLapTimeChanged()
        {
            BestLapChanged?.Invoke(this, new BestLapChangedEventArgs(BestLapTime));
        }

        private void OnCarClassBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (BestLapTime > TimeSpan.Zero && BestLapTime == e.LapTime)
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
            if (LapsCompleted != Drivers.Sum(x => x.LapsCompleted))
            {
                var activeDriver = Drivers.SingleOrDefault(x => x.IsActive == true);
                if (activeDriver != null)
                {
                    activeDriver.LapsCompleted = LapsCompleted - Drivers.Where(x => x.IsActive == false).Sum(x => x.LapsCompleted);
                }
            }
        }

        private void OnCurrentLapChanging()
        {
            if (CurrentLap?.Number > 0)
            {
                LastLap = CurrentLap;
            }
        }

        private void OnDriverBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (e.LapTime < (BestLapTime ?? TimeSpan.MaxValue))
            {
                BestLapTime = e.LapTime;
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
            if (LastLap.IsInLap == false && LastLap.IsOutLap == false && LastLap.IsValid == true && LastLap.Number > 1 && LastLap.Time < (activeDriver?.BestLap?.Time ?? TimeSpan.MaxValue))
            {
                activeDriver.BestLap = LastLap;
            }

            if (LastLap.Number > 0)
            {
                if (LastNLaps.Count() == _settings.NLaps)
                {
                    LastNLaps.RemoveAt(_settings.NLaps - 1);
                }

                LastNLaps.Insert(0, LastLap);
            }
        }

        private void OnLastNLapsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (BestNLaps.Count < LastNLaps.Count(x => x.IsInLap == false && x.IsOutLap == false && x.IsValid == true && x.Number > 1) || LastNLapsAverage < BestNLapsAverage)
            {
                BestNLaps.Clear();

                BestNLaps.AddRange(LastNLaps.Where(x => x.IsInLap == false && x.IsOutLap == false && x.IsValid == true && x.Number > 1).Select(x => x.Time));
            }
        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.NLaps))
            {
                if (BestNLaps.Count() > _settings.NLaps)
                {
                    foreach (var lap in BestNLaps.Skip(_settings.NLaps).ToList())
                    {
                        BestNLaps.Remove(lap);
                    }
                }

                if (LastNLaps.Count() > _settings.NLaps)
                {
                    foreach (var lap in LastNLaps.Skip(_settings.NLaps).ToList())
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
