using GameReaderCommon;
using IRacingReader;
using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Extensions;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.ViewModels;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Session : IDisposable
    {
        private static readonly double Weight = 1600 / Math.Log(2);

        private readonly IModifySimHub _plugin;
        private readonly TelemetryViewModel _telemetry;

        private ObservableCollection<CarClass> _carClasses;
        private short _counter;
        private string _description;
        private Game _game;

        public Session(IModifySimHub plugin, TelemetryViewModel telemetry)
        {
            _plugin = plugin;
            _plugin.DataUpdated += OnPluginDataUpdated;

            _telemetry = telemetry;

            CreateSimHubActions();
            CreateSimHubProperties();
        }

        private ObservableCollection<CarClass> CarClasses
        {
            get
            {
                if (_carClasses == null)
                {
                    _carClasses = new ObservableCollection<CarClass>();
                    _carClasses.CollectionChanged += OnCarClassesCollectionChanged;
                }

                return _carClasses;
            }
        }

        private string Description
        {
            get => _description;
            set
            {
                if (_description != value?.ToUpper())
                {
                    _description = value?.ToUpper();
                    OnDescriptionChanged();
                }
            }
        }

        private Game Game
        {
            get => _game;
            set
            {
                if (_game?.Name != value.Name)
                {
                    _game = value;
                    OnGameChanged();
                }
            }
        }

        private bool IsMultiClass => CarClasses.Count > 1;

        private bool IsPractice
        {
            get
            {
                switch (Description)
                {
                    case "OFFLINE TESTING":
                    case "OPEN PRACTICE":
                    case "PRACTICE":
                    case "WARMUP":
                        return true;
                    default:
                        return false;
                }
            }
        }

        private bool IsQualifying
        {
            get
            {
                switch (Description)
                {
                    case "LONE QUALIFY":
                    case "OPEN QUALIFY":
                    case "QUALIFY":
                        return true;
                    default:
                        return false;
                }
            }
        }

        private bool IsRace
        {
            get
            {
                switch (Description)
                {
                    case "RACE":
                        return true;
                    default:
                        return false;
                }
            }
        }

        private StatusDataBase StatusDatabase { get; set; }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_carClasses != null)
                {
                    foreach (var team in _carClasses.SelectMany(x => x.Teams))
                    {
                        team.Dispose();
                    }

                    _carClasses.CollectionChanged -= OnCarClassesCollectionChanged;
                }

                _plugin.DataUpdated -= OnPluginDataUpdated;
            }
        }

        private static MiniSector GetInterpolatedMiniSector(double trackPosition, Lap lap)
        {
            static long GetLinearInterpolation(double x, double x0, double x1, long y0, long y1)
            {
                if ((x1 - x0) == 0)
                {
                    return (y0 + y1) / 2;
                }

                return (long)(y0 + ((x - x0) * (y1 - y0) / (x1 - x0)));
            }

            if (trackPosition > 1)
            {
                trackPosition -= 1;
            }

            var nextSector = lap.MiniSectors.FirstOrDefault(x => x.TrackPosition >= trackPosition) ?? new MiniSector { Time = lap.Time, TrackPosition = 1 };
            var lastSector = lap.MiniSectors.OrderByDescending(x => x.TrackPosition).First(x => x.TrackPosition <= trackPosition);

            return new MiniSector
            {
                Time = TimeSpan.FromTicks(GetLinearInterpolation(trackPosition, lastSector.TrackPosition, nextSector.TrackPosition, lastSector.Time.Ticks, nextSector.Time.Ticks)),
                TrackPosition = trackPosition,
            };
        }

        private void CalculateDeltas()
        {
            var player = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            foreach (var carClass in CarClasses)
            {
                foreach (var team in carClass.Teams)
                {
                    team.DeltaToBest = (team.BestLap?.Time - carClass.Teams.Where(x => x.BestLap?.Time > TimeSpan.Zero).Min(x => x.BestLap?.Time))?.TotalSeconds ?? 0D;
                    team.DeltaToBestN = (team.BestNLapsAverage - carClass.Teams.Where(x => x.BestNLapsAverage > TimeSpan.Zero).Min(x => x.BestNLapsAverage))?.TotalSeconds ?? 0D;

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBest", team.DeltaToBest);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBestN", team.DeltaToBestN);

                    if (player != null)
                    {
                        team.DeltaToPlayerBest = (team.BestLap?.Time - player.BestLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerBestN = (team.BestNLapsAverage - player.BestNLapsAverage)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLast = (team.LastLap?.Time - player.LastLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLastN = (team.LastNLapsAverage - player.LastNLapsAverage)?.TotalSeconds ?? 0D;

                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBest", team.DeltaToPlayerBest);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBestN", team.DeltaToPlayerBestN);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLast", team.DeltaToPlayerLast);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLastN", team.DeltaToPlayerLastN);
                    }
                }
            }
        }

        private void CalculateEstimatedLapTimes(ReferenceLap referenceLap)
        {
            foreach (var carClass in CarClasses)
            {
                foreach (var team in carClass.Teams)
                {
                    var miniSector = team.CurrentLap.LastMiniSector;

                    switch (referenceLap)
                    {
                        case ReferenceLap.PersonalBest:
                            if (team.ActiveDriver?.BestLap != null && team.CurrentLap.MiniSectors.Any())
                            {
                                team.EstimatedLapTime = team.ActiveDriver.BestLap.Time + (miniSector.Time - GetInterpolatedMiniSector(miniSector.TrackPosition, team.ActiveDriver.BestLap).Time);
                            }
                            else
                            {
                                team.EstimatedLapTime = null;
                            }

                            break;
                        case ReferenceLap.TeamBest:
                            if (team.BestLap != null && team.CurrentLap.MiniSectors.Any())
                            {
                                team.EstimatedLapTime = team.BestLap.Time + (miniSector.Time - GetInterpolatedMiniSector(miniSector.TrackPosition, team.BestLap).Time);
                            }
                            else
                            {
                                team.EstimatedLapTime = null;
                            }

                            break;
                        case ReferenceLap.TeamBestN:
                            if (team.BestNLaps.Any())
                            {
                                var estimatedLaps = new List<TimeSpan>();

                                foreach (var lap in team.BestNLaps)
                                {
                                    estimatedLaps.Add(lap.Time + (miniSector.Time - GetInterpolatedMiniSector(miniSector.TrackPosition, lap).Time));
                                }

                                team.EstimatedLapTime = TimeSpan.FromSeconds(estimatedLaps.Average(x => x.TotalSeconds));
                            }
                            else
                            {
                                team.EstimatedLapTime = null;
                            }

                            break;
                        case ReferenceLap.TeamLast:
                            if (team.LastLap != null && team.CurrentLap.MiniSectors.Any())
                            {
                                team.EstimatedLapTime = team.LastLap.Time + (miniSector.Time - GetInterpolatedMiniSector(miniSector.TrackPosition, team.LastLap).Time);
                            }
                            else
                            {
                                team.EstimatedLapTime = null;
                            }

                            break;
                        case ReferenceLap.TeamLastN:
                            if (team.LastNLaps.Any())
                            {
                                var estimatedLaps = new List<TimeSpan>();

                                foreach (var lap in team.LastNLaps)
                                {
                                    estimatedLaps.Add(lap.Time + (miniSector.Time - GetInterpolatedMiniSector(miniSector.TrackPosition, lap).Time));
                                }

                                team.EstimatedLapTime = TimeSpan.FromSeconds(estimatedLaps.Average(x => x.TotalSeconds));
                            }
                            else
                            {
                                team.EstimatedLapTime = null;
                            }

                            break;
                        case ReferenceLap.ClassBest:
                            if (carClass.BestLap != null && team.CurrentLap.MiniSectors.Any())
                            {
                                team.EstimatedLapTime = carClass.BestLap.Time + (miniSector.Time - GetInterpolatedMiniSector(miniSector.TrackPosition, carClass.BestLap).Time);
                            }
                            else
                            {
                                team.EstimatedLapTime = null;
                            }

                            break;
                        default:
                            throw new InvalidEnumArgumentException(nameof(referenceLap), (int)referenceLap, typeof(ReferenceLap));
                    }

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedDelta", team.EstimatedDelta);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapColor", team.EstimatedLapColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapTime", team.EstimatedLapTime ?? TimeSpan.Zero);
                }
            }
        }

        private void CalculateGaps(bool enableGapCalculations)
        {
            static double GetGap(Team team, Team comparison, double? defaultValue)
            {
                if (team.CurrentLapHighPrecision < comparison.CurrentLapHighPrecision)
                {
                    var lap = team.ActiveDriver?.BestLap;
                    if (lap == null && team.LastLap?.IsInLap == false && team.LastLap?.IsOutLap == false && team.LastLap?.Time > TimeSpan.Zero)
                    {
                        lap = team.LastLap;
                    }

                    if (lap != null)
                    {
                        double gap = 0D;

                        var laps = comparison.CurrentLapHighPrecision - team.CurrentLapHighPrecision;
                        if (laps > 1)
                        {
                            gap = (int)laps * lap.Time.TotalSeconds;
                        }

                        if (team.CurrentLap.MiniSectors.Any() && comparison.CurrentLap.MiniSectors.Any())
                        {
                            var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);
                            var comparisonMiniSector = GetInterpolatedMiniSector(comparison.CurrentLap.LastMiniSector.TrackPosition, lap);

                            if (comparisonMiniSector.TrackPosition > teamMiniSector.TrackPosition)
                            {
                                gap += (comparisonMiniSector.Time - teamMiniSector.Time).TotalSeconds;
                            }
                            else if (comparisonMiniSector.TrackPosition < teamMiniSector.TrackPosition)
                            {
                                gap += (lap.Time - (teamMiniSector.Time - comparisonMiniSector.Time)).TotalSeconds;
                            }
                        }

                        return gap;
                    }
                }
                else if (team.CurrentLapHighPrecision > comparison.CurrentLapHighPrecision)
                {
                    var lap = comparison.ActiveDriver?.BestLap;
                    if (lap == null && comparison.LastLap?.IsInLap == false && comparison.LastLap?.IsOutLap == false && comparison.LastLap?.Time > TimeSpan.Zero)
                    {
                        lap = comparison.LastLap;
                    }

                    if (lap != null)
                    {
                        double gap = 0D;

                        var laps = comparison.CurrentLapHighPrecision - team.CurrentLapHighPrecision;
                        if (laps < -1)
                        {
                            gap = (int)laps * lap.Time.TotalSeconds;
                        }

                        if (team.CurrentLap.MiniSectors.Any() && comparison.CurrentLap.MiniSectors.Any())
                        {
                            var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);
                            var comparisonMiniSector = GetInterpolatedMiniSector(comparison.CurrentLap.LastMiniSector.TrackPosition, lap);

                            if (teamMiniSector.TrackPosition > comparisonMiniSector.TrackPosition)
                            {
                                gap -= (teamMiniSector.Time - comparisonMiniSector.Time).TotalSeconds;
                            }
                            else if (teamMiniSector.TrackPosition < comparisonMiniSector.TrackPosition)
                            {
                                gap -= (lap.Time - (comparisonMiniSector.Time - teamMiniSector.Time)).TotalSeconds;
                            }
                        }

                        return gap;
                    }
                }
                else
                {
                    return 0D;
                }

                return defaultValue ?? 0D;
            }

            static string GetGapAsString(Team team, Team comparison, double? gap, bool inverse)
            {
                var laps = team.CurrentLapHighPrecision - comparison.CurrentLapHighPrecision;
                if (laps > 1 || laps < -1)
                {
                    return inverse == true ? $"{laps:+0;-0}L" : $"{laps:-0;+0}L";
                }
                else
                {
                    if (gap != 0)
                    {
                        return inverse == true ? $"{gap:-0.0;+0.0}" : $"{gap:+0.0;-0.0}";
                    }
                    else
                    {
                        return "0.0";
                    }
                }
            }

            static string GetIntervalAsString(Team team, Team comparison, double? interval)
            {
                var laps = comparison.CurrentLapHighPrecision - team.CurrentLapHighPrecision;
                if (laps > 1)
                {
                    return $"{laps:+0}L";
                }
                else
                {
                    return $"{interval:+0.0;-0.0}";
                }
            }

            static double? GetRelativeGap(Team team, Team comparison, double? defaultValue)
            {
                if (defaultValue > 0)
                {
                    var lap = team.ActiveDriver?.BestLap;
                    if (lap == null && team.LastLap?.IsInLap == false && team.LastLap?.IsOutLap == false && team.LastLap?.Time > TimeSpan.Zero)
                    {
                        lap = team.LastLap;
                    }

                    if (lap != null)
                    {
                        if (team.CurrentLap.MiniSectors.Any() && comparison.CurrentLap.MiniSectors.Any())
                        {
                            var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);
                            var comparisonMiniSector = GetInterpolatedMiniSector(comparison.CurrentLap.LastMiniSector.TrackPosition, lap);

                            if (comparisonMiniSector.TrackPosition > teamMiniSector.TrackPosition)
                            {
                                return (comparisonMiniSector.Time - teamMiniSector.Time).TotalSeconds;
                            }
                            else if (comparisonMiniSector.TrackPosition < teamMiniSector.TrackPosition)
                            {
                                return (lap.Time - (teamMiniSector.Time - comparisonMiniSector.Time)).TotalSeconds;
                            }
                        }
                    }
                }
                else if (defaultValue < 0)
                {
                    var lap = comparison.ActiveDriver?.BestLap;
                    if (lap == null && comparison.LastLap?.IsInLap == false && comparison.LastLap?.IsOutLap == false && comparison.LastLap?.Time > TimeSpan.Zero)
                    {
                        lap = comparison.LastLap;
                    }

                    if (lap != null)
                    {
                        if (team.CurrentLap.MiniSectors.Any() && comparison.CurrentLap.MiniSectors.Any())
                        {
                            var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);
                            var comparisonMiniSector = GetInterpolatedMiniSector(comparison.CurrentLap.LastMiniSector.TrackPosition, lap);

                            if (teamMiniSector.TrackPosition > comparisonMiniSector.TrackPosition)
                            {
                                return -(teamMiniSector.Time - comparisonMiniSector.Time).TotalSeconds;
                            }
                            else if (teamMiniSector.TrackPosition < comparisonMiniSector.TrackPosition)
                            {
                                return -(lap.Time - (comparisonMiniSector.Time - teamMiniSector.Time)).TotalSeconds;
                            }
                        }
                    }
                }

                return defaultValue;
            }

            var leader = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.LivePosition == 1);
            var player = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            Parallel.ForEach(CarClasses, carClass =>
            {
                var classLeader = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == 1);

                foreach (var team in carClass.Teams.OrderBy(x => x.LivePositionInClass))
                {
                    if (leader != null)
                    {
                        if (enableGapCalculations == true)
                        {
                            team.GapToLeader = GetGap(team, leader, StatusDatabase.Opponents.GetUnique(team, Game)?.GaptoLeader);
                        }

                        team.GapToLeaderString = GetGapAsString(team, leader, team.GapToLeader, _telemetry.EnableInverseGapStrings);

                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeader", team.GapToLeader);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeaderString", team.GapToLeaderString);
                    }

                    if (classLeader != null)
                    {
                        if (enableGapCalculations == true)
                        {
                            team.GapToClassLeader = GetGap(team, classLeader, StatusDatabase.Opponents.GetUnique(team, Game)?.GaptoClassLeader);
                        }

                        team.GapToClassLeaderString = GetGapAsString(team, classLeader, team.GapToClassLeader, _telemetry.EnableInverseGapStrings);

                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToClassLeader", team.GapToClassLeader);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToClassLeaderString", team.GapToClassLeaderString);
                    }

                    if (player != null)
                    {
                        if (enableGapCalculations == true)
                        {
                            team.GapToPlayer = GetGap(team, player, StatusDatabase.Opponents.GetUnique(team, Game)?.GaptoPlayer);
                            team.RelativeGapToPlayer = GetRelativeGap(team, player, StatusDatabase.Opponents.GetUnique(team, Game)?.RelativeGapToPlayer);
                        }

                        team.GapToPlayerString = GetGapAsString(team, player, team.GapToPlayer, _telemetry.EnableInverseGapStrings);

                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayer", team.GapToPlayer);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayerString", team.GapToPlayerString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayer", team.RelativeGapToPlayer);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerColor", IsRace ? team.RelativeGapToPlayerColor : Colors.White);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerString", team.RelativeGapToPlayerString);
                    }

                    if (team.LivePosition == 1)
                    {
                        team.Interval = 0d;
                        team.IntervalString = $"L{team.LapsCompleted + 1}";
                    }
                    else
                    {
                        var teamAhead = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.LivePosition == team.LivePosition - 1);
                        if (teamAhead != null)
                        {
                            team.Interval = team.GapToLeader - teamAhead.GapToLeader;
                            team.IntervalString = GetIntervalAsString(team, teamAhead, team.Interval);
                        }
                    }

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_Interval", team.Interval);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalString", team.IntervalString);

                    if (team.LivePositionInClass == 1)
                    {
                        team.IntervalInClass = 0d;
                        team.IntervalInClassString = $"L{team.LapsCompleted + 1}";
                    }
                    else
                    {
                        var teamAhead = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == team.LivePositionInClass - 1);
                        if (teamAhead != null)
                        {
                            team.IntervalInClass = team.GapToClassLeader - teamAhead.GapToClassLeader;
                            team.IntervalInClassString = GetIntervalAsString(team, teamAhead, team.IntervalInClass);
                        }
                    }

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalInClass", team.IntervalInClass);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalInClassString", team.IntervalInClassString);
                }
            });
        }

        private void CalculateIRating()
        {
            static double GetIRatingChange(int teamIRating, int position, IEnumerable<int> iRatings)
            {
                double factor = ((iRatings.Count() / 2D) - position) / 100D;
                double sum = -0.5;

                foreach (var iRating in iRatings)
                {
                    sum += (1 - Math.Exp(-teamIRating / Weight)) * Math.Exp(-iRating / Weight) / (((1 - Math.Exp(-iRating / Weight)) * Math.Exp(-teamIRating / Weight)) + ((1 - Math.Exp(-teamIRating / Weight)) * Math.Exp(-iRating / Weight)));
                }

                return Math.Round((iRatings.Count() - position - sum - factor) * 200 / iRatings.Count());
            }

            Parallel.ForEach(CarClasses, carClass =>
            {
                Parallel.ForEach(carClass.Teams.Where(x => x.IRating > 0), team =>
                {
                    if (IsQualifying == true || IsRace == true)
                    {
                        var iRatingChange = GetIRatingChange(team.IRating.Value, team.LivePositionInClass, carClass.Teams.Where(x => x.IRating > 0).Select(x => x.IRating.Value));

                        if (team.Drivers.Count > 1)
                        {
                            foreach (var driver in team.Drivers.Where(x => x.IRating > 0))
                            {
                                if (driver.LapsCompleted > 0)
                                {
                                    driver.IRatingChange = (int)(iRatingChange * driver.LapsCompleted / team.LapsCompleted);
                                }
                                else
                                {
                                    driver.IRatingChange = 0;
                                }
                            }
                        }
                        else
                        {
                            team.Drivers.Single().IRatingChange = (int)iRatingChange;
                        }

                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingChange", team.ActiveDriver.IRatingChange);
                    }

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRating", team.ActiveDriver.IRating);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingLicenseCombinedString", team.ActiveDriver.IRatingLicenseCombinedString);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingString", team.ActiveDriver.IRatingString);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamIRating", team.IRating);
                });

                _plugin.SetProperty($"Class_{carClass.Index:D2}_SoF", carClass.StrengthOfField);
                _plugin.SetProperty($"Class_{carClass.Index:D2}_SoFString", carClass.StrengthOfFieldString);
            });
        }

        private void CalculateLivePositions()
        {
            foreach (var (team, i) in CarClasses.SelectMany(x => x.Teams).OrderByDescending(x => x.CurrentLapHighPrecision).Select((team, i) => (team, i)))
            {
                if (IsRace == true)
                {
                    team.LivePosition = i + 1;
                }
                else
                {
                    team.LivePosition = team.LeaderboardPosition;
                }

                _plugin.SetProperty($"Drivers_Live_{team.LivePosition:D2}_LeaderboardPosition", team.LeaderboardPosition);
                _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePosition", team.LivePosition);
                _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_PositionsGained", team.PositionsGained);
            }

            foreach (var carClass in CarClasses)
            {
                foreach (var team in carClass.Teams)
                {
                    team.LivePositionInClass = carClass.Teams.Count(x => x.LivePosition <= team.LivePosition);

                    _plugin.SetProperty($"Class_{carClass.Index:D2}_{team.LivePositionInClass:D2}_LeaderboardPosition", team.LeaderboardPosition);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePositionInClass", team.LivePositionInClass);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_PositionsGainedInClass", team.PositionsGainedInClass);
                }
            }
        }

        private void CreateSimHubActions()
        {
            _plugin.AddAction("DecrementNLaps", (a, b) => _telemetry.NLaps--);
            _plugin.AddAction("IncrementNLaps", (a, b) => _telemetry.NLaps++);
            _plugin.AddAction("LastReferenceLap", (a, b) => _telemetry.ReferenceLap--);
            _plugin.AddAction("NextReferenceLap", (a, b) => _telemetry.ReferenceLap++);
            _plugin.AddAction("ResetBestLaps", ResetBestLaps);
            _plugin.AddAction("ToggleJSOverrides", (a, b) => _telemetry.OverrideJavaScriptFunctions = _telemetry.OverrideJavaScriptFunctions == false);
        }

        private void CreateSimHubProperties()
        {
            for (int i = 1; i <= 63; i++)
            {
                _plugin.AddProperty($"Drivers_{i:D2}_BestNLapsAverage", TimeSpan.Zero);
                _plugin.AddProperty($"Drivers_{i:D2}_BestNLapsColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_BestLapColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                _plugin.AddProperty($"Drivers_{i:D2}_CarNumber", -1);
                _plugin.AddProperty($"Drivers_{i:D2}_ClassColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_ClassIndex", -1);
                _plugin.AddProperty($"Drivers_{i:D2}_ClassString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_ClassTextColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_CurrentLapTime", TimeSpan.Zero);
                _plugin.AddProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_DeltaToBestN", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_DeltaToPlayerBestN", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_DeltaToPlayerLastN", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_EstimatedDelta", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_EstimatedLapColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_EstimatedLapTime", TimeSpan.Zero);
                _plugin.AddProperty($"Drivers_{i:D2}_GapToClassLeader", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_GapToClassLeaderString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_GapToLeader", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_GapToLeaderString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_GapToPlayerString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_GridPosition", -1);
                _plugin.AddProperty($"Drivers_{i:D2}_GridPositionInClass", -1);
                _plugin.AddProperty($"Drivers_{i:D2}_Interval", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_IntervalInClass", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_IntervalInClassString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_IntervalString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_IRating", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_IRatingChange", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_IRatingString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_IsConnected", false);
                _plugin.AddProperty($"Drivers_{i:D2}_IsInPit", false);
                _plugin.AddProperty($"Drivers_{i:D2}_IsPlayer", false);
                _plugin.AddProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_LastNLapsAverage", TimeSpan.Zero);
                _plugin.AddProperty($"Drivers_{i:D2}_LastNLapsColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_LastLapColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                _plugin.AddProperty($"Drivers_{i:D2}_LeaderboardPosition", -1);
                _plugin.AddProperty($"Drivers_{i:D2}_LicenseColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_LicenseShortString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_LicenseString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_LicenseTextColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_LivePosition", -1);
                _plugin.AddProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                _plugin.AddProperty($"Drivers_{i:D2}_Name", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_PositionsGained", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_PositionsGainedInClass", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_ShortName", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_TeamBestLapColor", string.Empty);
                _plugin.AddProperty($"Drivers_{i:D2}_TeamBestLapTime", TimeSpan.Zero);
                _plugin.AddProperty($"Drivers_{i:D2}_TeamIRating", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                _plugin.AddProperty($"Drivers_{i:D2}_TeamName", string.Empty);

                _plugin.AddProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= 7; i++)
            {
                _plugin.AddProperty($"Class_{i:D2}_OpponentCount", 0);
                _plugin.AddProperty($"Class_{i:D2}_SoF", 0);
                _plugin.AddProperty($"Class_{i:D2}_SoFString", string.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    _plugin.AddProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            _plugin.AddProperty("Game_IsSupported", "Untested");
            _plugin.AddProperty("Game_Name", string.Empty);
            _plugin.AddProperty("Player_Incidents", 0);
            _plugin.AddProperty("Session_Description", string.Empty);
            _plugin.AddProperty("Session_IsMultiClass", false);
            _plugin.AttachDelegate("Settings_NLaps", () => _telemetry.NLaps);
            _plugin.AttachDelegate("Settings_OverrideJavaScriptFunctions", () => _telemetry.OverrideJavaScriptFunctions);
            _plugin.AttachDelegate("Settings_ReferenceLap", () => _telemetry.ReferenceLap);
        }

        private void GenerateMiniSectors()
        {
            Parallel.ForEach(StatusDatabase.Opponents, opponent =>
            {
                var team = CarClasses.SelectMany(x => x.Teams).GetUnique(opponent, Game);
                if (team != null)
                {
                    if (team.IsPlayer == true || IsQualifying == false)
                    {
                        if (opponent.CurrentLap > team.CurrentLap.Number)
                        {
                            team.CurrentLap.IsInLap = opponent.IsCarInPitLane;

                            team.CurrentLap.MiniSectors.RemoveAll(x => x.TrackPosition >= 1);
                            team.CurrentLap.MiniSectors.Add(new MiniSector
                            {
                                Time = opponent.LastLapTime,
                                TrackPosition = 1,
                            });

                            team.CurrentLap = new Lap(opponent.CurrentLap.Value)
                            {
                                IsOutLap = opponent.IsCarInPitLane,
                            };
                        }

                        if (team.CurrentLap.IsDirty == false)
                        {
                            team.CurrentLap.IsDirty = opponent.LapValid == false;
                        }

                        team.CurrentLap.MiniSectors.RemoveAll(x => x.TrackPosition >= opponent.TrackPositionPercent);
                        team.CurrentLap.MiniSectors.Add(new MiniSector
                        {
                            Time = opponent.CurrentLapTime ?? TimeSpan.Zero,
                            TrackPosition = opponent.TrackPositionPercent.Value,
                        });
                    }

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapTime", team.CurrentLap.Time);
                }
            });
        }

        private void GetGameData()
        {
            Description = StatusDatabase.SessionTypeName;

            foreach (var (opponent, i) in StatusDatabase.Opponents.Select((opponent, i) => (opponent, i)))
            {
                var carClass = CarClasses.SingleOrDefault(x => x.Name == opponent.CarClass);
                if (carClass == null)
                {
                    carClass = new CarClass(_plugin)
                    {
                        Color = opponent.CarClassColor,
                        Index = CarClasses.Count() + 1,
                        Name = opponent.CarClass,
                        TextColor = opponent.CarClassTextColor,
                    };

                    CarClasses.Add(carClass);
                }

                var team = carClass.Teams.GetUnique(opponent, Game);
                if (team == null)
                {
                    team = new Team(carClass, _telemetry)
                    {
                        CarNumber = opponent.CarNumber,
                        CurrentLap = new Lap(opponent.CurrentLap ?? 0)
                        {
                            IsOutLap = opponent.IsCarInPitLane,
                        },
                        CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                        IsInPit = opponent.IsCarInPitLane,
                        Name = opponent.TeamName,
                    };

                    _plugin.AttachDelegate($"Drivers_Car_{team.CarNumber}_LeaderboardPosition", () => { return team.LeaderboardPosition; });

                    carClass.Teams.Add(team);
                }
                else if (opponent.IsConnected == true)
                {
                    team.CurrentLapHighPrecision = opponent.CurrentLapHighPrecision;
                    team.IsInPit = opponent.IsCarInPitLane;
                }
                else if (opponent.IsConnected == false)
                {
                    team.IsInPit = true;
                }

                team.IsConnected = opponent.IsConnected;
                team.IsPlayer = opponent.IsPlayer;

                _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapHighPrecision", team.CurrentLapHighPrecision);
                _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsConnected", team.IsConnected);
                _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsInPit", team.IsInPit);

                if (IsQualifying == true && opponent.BestLapTime > TimeSpan.Zero && opponent.BestLapTime < (team.BestLap?.Time ?? TimeSpan.MaxValue))
                {
                    team.BestLap = new Lap((opponent.CurrentLap - 1) ?? 0);

                    team.BestLap.MiniSectors.Add(new MiniSector
                    {
                        Time = opponent.BestLapTime,
                        TrackPosition = 1,
                    });
                }

                if (IsRace == true && StatusDatabase.Flag_Green == 1 && team.CurrentLapHighPrecision < 1)
                {
                    if (Game.IsIRacing == true)
                    {
                        team.GridPosition = team.LeaderboardPosition;
                        team.GridPositionInClass = carClass.Teams.Count(x => x.LeaderboardPosition <= team.LeaderboardPosition);
                    }
                    else if (team.GridPosition == -1)
                    {
                        team.GridPosition = opponent.StartPosition ?? -1;
                        team.GridPositionInClass = opponent.StartPositionClass ?? -1;
                    }

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GridPosition", team.GridPosition);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GridPositionInClass", team.GridPositionInClass);
                }

                if (_telemetry.EnableGapCalculations == false)
                {
                    team.GapToClassLeader = opponent.GaptoClassLeader ?? 0D;
                    team.GapToLeader = opponent.GaptoLeader ?? 0D;
                    team.GapToPlayer = opponent.GaptoPlayer ?? 0D;
                    team.RelativeGapToPlayer = opponent.IsConnected ? opponent.RelativeGapToPlayer : null;
                }

                var driver = team.Drivers.SingleOrDefault(x => x.Name == opponent.Name);
                if (driver == null)
                {
                    driver = new Driver(carClass, team.IsPlayer)
                    {
                        IRating = opponent.IRacing_IRating,
                        License = new License
                        {
                            String = opponent.LicenceString,
                        },
                        Name = opponent.Name,
                    };

                    team.Drivers.Add(driver);
                }

                team.ActiveDriver = driver;
            }

            if (StatusDatabase.GetRawDataObject() is DataSampleEx iRacingData)
            {
                iRacingData.Telemetry.TryGetValue("PlayerCarTeamIncidentCount", out object rawIncidents);

                _plugin.SetProperty("Player_Incidents", Convert.ToInt32(rawIncidents));
            }
        }

        private void OnCarClassesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (var team in e.OldItems.OfType<CarClass>().SelectMany(x => x.Teams))
                {
                    team.Dispose();
                }
            }

            _plugin.SetProperty("Session_IsMultiClass", IsMultiClass);
        }

        private void OnDescriptionChanged()
        {
            ResetSession();

            _plugin.SetProperty("Session_Description", Description);
        }

        private void OnGameChanged()
        {
            if (Game.IsSupported != null)
            {
                _plugin.SetProperty("Game_IsSupported", Game.IsSupported);
            }
            else
            {
                _plugin.SetProperty("Game_IsSupported", "Untested");
            }

            _plugin.SetProperty("Game_Name", Game.Name);
        }

        private void OnPluginDataUpdated(object sender, NotifyDataUpdatedEventArgs e)
        {
            _counter++;

            if (_counter > 59)
            {
                _counter = 0;
            }

            if (e.Data.GameRunning && e.Data.NewData != null)
            {
                Game = new Game(e.Data.GameName);

                if (Game.IsSupported != false)
                {
                    StatusDatabase = e.Data.NewData;

                    // 0, 4, 8, 12, 16...
                    if (_counter % 4 == 0)
                    {
                        GetGameData();
                    }

                    // 0
                    if (_counter % 60 == 0)
                    {
                        CalculateLivePositions();
                    }

                    // 0, 30
                    if (_counter % 30 == 0)
                    {
                        GenerateMiniSectors();
                    }

                    // 1, 3, 5, 7, 9...
                    if (_counter % 2 == 1)
                    {
                        WriteSimHubData();
                    }

                    // 0, 6, 12, 18, 24...
                    if (_counter % 6 == 0)
                    {
                        CalculateGaps(_telemetry.EnableGapCalculations);
                    }

                    // 2, 8, 14, 20, 26...
                    if (_counter % 6 == 2)
                    {
                        CalculateEstimatedLapTimes(_telemetry.ReferenceLap);
                    }

                    // 4, 34
                    if (_counter % 30 == 4)
                    {
                        CalculateDeltas();
                    }

                    // 36
                    if (_counter % 60 == 36 && Game.IsIRacing == true)
                    {
                        CalculateIRating();
                    }
                }
            }
            else
            {
                ResetSession();
            }
        }

        private void ResetBestLaps(PluginManager _, string __)
        {
            foreach (var driver in CarClasses.SelectMany(x => x.Teams).SelectMany(x => x.Drivers))
            {
                driver.BestLap = null;
            }
        }

        private void ResetDriverProperties(int start)
        {
            for (int i = start; i <= 63; i++)
            {
                _plugin.SetProperty($"Drivers_{i:D2}_BestNLapsAverage", TimeSpan.Zero);
                _plugin.SetProperty($"Drivers_{i:D2}_BestNLapsColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_BestLapColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                _plugin.SetProperty($"Drivers_{i:D2}_CarNumber", -1);
                _plugin.SetProperty($"Drivers_{i:D2}_ClassColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_ClassIndex", -1);
                _plugin.SetProperty($"Drivers_{i:D2}_ClassString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_ClassTextColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_CurrentLapTime", TimeSpan.Zero);
                _plugin.SetProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_DeltaToBestN", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_DeltaToPlayerBestN", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_DeltaToPlayerLastN", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_EstimatedDelta", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_EstimatedLapColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_EstimatedLapTime", TimeSpan.Zero);
                _plugin.SetProperty($"Drivers_{i:D2}_GapToClassLeader", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_GapToClassLeaderString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_GapToLeader", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_GapToLeaderString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_GapToPlayerString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_GridPosition", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_GridPositionInClass", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_Interval", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_IntervalInClass", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_IntervalInClassString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_IntervalString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_IRating", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_IRatingChange", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_IRatingString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_IsConnected", false);
                _plugin.SetProperty($"Drivers_{i:D2}_IsInPit", false);
                _plugin.SetProperty($"Drivers_{i:D2}_IsPlayer", false);
                _plugin.SetProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_LastNLapsAverage", TimeSpan.Zero);
                _plugin.SetProperty($"Drivers_{i:D2}_LastNLapsColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_LastLapColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                _plugin.SetProperty($"Drivers_{i:D2}_LeaderboardPosition", -1);
                _plugin.SetProperty($"Drivers_{i:D2}_LicenseColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_LicenseShortString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_LicenseString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_LicenseTextColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_LivePosition", -1);
                _plugin.SetProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                _plugin.SetProperty($"Drivers_{i:D2}_Name", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_PositionsGained", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_PositionsGainedInClass", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_ShortName", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_TeamBestLapColor", string.Empty);
                _plugin.SetProperty($"Drivers_{i:D2}_TeamBestLapTime", TimeSpan.Zero);
                _plugin.SetProperty($"Drivers_{i:D2}_TeamIRating", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                _plugin.SetProperty($"Drivers_{i:D2}_TeamName", string.Empty);

                _plugin.SetProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }
        }

        private void ResetSession()
        {
            CarClasses.Clear();
            ResetSimHubProperties();
        }

        private void ResetSimHubProperties()
        {
            ResetDriverProperties(1);

            for (int i = 1; i <= 7; i++)
            {
                _plugin.SetProperty($"Class_{i:D2}_OpponentCount", 0);
                _plugin.SetProperty($"Class_{i:D2}_SoF", 0);
                _plugin.SetProperty($"Class_{i:D2}_SoFString", string.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    _plugin.SetProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            _plugin.SetProperty("Player_Incidents", 0);
            _plugin.SetProperty("Session_Description", string.Empty);
            _plugin.SetProperty("Session_IsMultiClass", false);
        }

        private void WriteSimHubData()
        {
            foreach (var (opponent, i) in StatusDatabase.Opponents.Select((opponent, i) => (opponent, i)))
            {
                var team = CarClasses.SelectMany(x => x.Teams).GetUnique(opponent, Game);
                if (team != null)
                {
                    team.LeaderboardPosition = i + 1;
                }
            }

            var teamsToRemove = CarClasses.SelectMany(x => x.Teams).GetAbsent(StatusDatabase.Opponents, Game).ToList();
            if (teamsToRemove.Count > 0)
            {
                foreach (var carClass in CarClasses.Where(x => x.Teams.Any(y => teamsToRemove.Contains(y))))
                {
                    foreach (var team in teamsToRemove.OrderByDescending(x => x.LivePosition))
                    {
                        CarClasses.SelectMany(x => x.Teams).Where(x => x.LivePosition > team.LivePosition).ToList().ForEach(x => x.LivePosition--);
                        carClass.Teams.Where(x => x.LivePositionInClass > team.LivePositionInClass).ToList().ForEach(x => x.LivePositionInClass--);
                        carClass.Teams.Remove(team);
                        team.LeaderboardPosition = -1;
                        team.Dispose();
                    }
                }

                ResetDriverProperties(CarClasses.SelectMany(x => x.Teams).Count() + 1);
            }

            Parallel.ForEach(CarClasses, carClass =>
            {
                Parallel.ForEach(carClass.Teams, team =>
                {
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestNLapsAverage", team.BestNLapsAverage ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestNLapsColor", team.BestNLapsColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapColor", team.ActiveDriver.BestLapColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapTime", team.ActiveDriver.BestLap?.Time ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LapsCompleted", team.ActiveDriver.LapsCompleted);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastNLapsAverage", team.LastNLapsAverage ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastNLapsColor", team.LastNLapsColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapColor", team.LastLapColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapTime", team.LastLap?.Time ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapColor", team.BestLapColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapTime", team.BestLap?.Time ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamLapsCompleted", team.LapsCompleted);

                    if (team.IsDirty == true)
                    {
                        _plugin.SetProperty($"Class_{carClass.Index:D2}_{team.LivePositionInClass:D2}_LeaderboardPosition", team.LeaderboardPosition);

                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CarNumber", team.CarNumber);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassColor", carClass.Color);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassIndex", carClass.Index);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassString", carClass.ShortName);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassTextColor", carClass.TextColor);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapHighPrecision", team.CurrentLapHighPrecision);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapTime", team.CurrentLap.Time);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBest", team.DeltaToBest);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBestN", team.DeltaToBestN);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBest", team.DeltaToPlayerBest);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBestN", team.DeltaToPlayerBestN);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLast", team.DeltaToPlayerLast);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLastN", team.DeltaToPlayerLastN);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedDelta", team.EstimatedDelta);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapColor", team.EstimatedLapColor);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapTime", team.EstimatedLapTime ?? TimeSpan.Zero);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToClassLeader", team.GapToClassLeader);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToClassLeaderString", team.GapToClassLeaderString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeader", team.GapToLeader);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeaderString", team.GapToLeaderString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayer", team.GapToPlayer);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayerString", team.GapToPlayerString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GridPosition", team.GridPosition);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GridPositionInClass", team.GridPositionInClass);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_Interval", team.Interval);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalInClass", team.IntervalInClass);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalInClassString", team.IntervalInClassString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalString", team.IntervalString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsConnected", team.IsConnected);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsInPit", team.IsInPit);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsPlayer", team.IsPlayer);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LeaderboardPosition", team.LeaderboardPosition);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseColor", team.ActiveDriver.License.Color);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseShortString", team.ActiveDriver.License.ShortString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseString", team.ActiveDriver.License.String);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseTextColor", team.ActiveDriver.License.TextColor);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePosition", team.LivePosition);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePositionInClass", team.LivePositionInClass);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_Name", team.ActiveDriver.Name);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_PositionsGained", team.PositionsGained);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_PositionsGainedInClass", team.PositionsGainedInClass);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayer", team.RelativeGapToPlayer);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerColor", IsRace ? team.RelativeGapToPlayerColor : Colors.White);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerString", team.RelativeGapToPlayerString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ShortName", team.ActiveDriver.ShortName);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamName", team.Name);

                        _plugin.SetProperty($"Drivers_Live_{team.LivePosition:D2}_LeaderboardPosition", team.LeaderboardPosition);

                        team.IsDirty = false;
                    }
                });
            });
        }

        #region Interface: IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}