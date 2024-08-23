using GameReaderCommon;
using IRacingReader;
using PostItNoteRacing.Common;
using PostItNoteRacing.Common.Extensions;
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
    internal class Session : DisposableObject
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

        public event EventHandler RequestNew;

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
                    OnDescriptionChanging();
                    _description = value?.ToUpper();
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_carClasses != null)
                {
                    foreach (var carClass in _carClasses)
                    {
                        foreach (var team in carClass.Teams)
                        {
                            foreach (var driver in team.Drivers)
                            {
                                driver.Dispose();
                            }

                            team.Dispose();
                        }

                        carClass.Dispose();
                    }

                    _carClasses.RemoveAll();
                    _carClasses.CollectionChanged -= OnCarClassesCollectionChanged;
                }

                _plugin.DetachDelegate("Game_IsSupported");
                _plugin.DetachDelegate("Game_Name");
                _plugin.DetachDelegate("Session_Description");
                _plugin.DetachDelegate("Session_IsMultiClass");

                _plugin.DataUpdated -= OnPluginDataUpdated;
            }

            base.Dispose(disposing);
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

            var nextSector = lap.MiniSectors.OrderBy(x => x.TrackPosition).FirstOrDefault(x => x.TrackPosition >= trackPosition) ?? new MiniSector { Time = lap.Time, TrackPosition = 1 };
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

                    if (player != null)
                    {
                        team.DeltaToPlayerBest = (team.BestLap?.Time - player.BestLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerBestN = (team.BestNLapsAverage - player.BestNLapsAverage)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLast = (team.LastLap?.Time - player.LastLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLastN = (team.LastNLapsAverage - player.LastNLapsAverage)?.TotalSeconds ?? 0D;
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
                    }

                    if (classLeader != null)
                    {
                        if (enableGapCalculations == true)
                        {
                            team.GapToClassLeader = GetGap(team, classLeader, StatusDatabase.Opponents.GetUnique(team, Game)?.GaptoClassLeader);
                        }

                        team.GapToClassLeaderString = GetGapAsString(team, classLeader, team.GapToClassLeader, _telemetry.EnableInverseGapStrings);
                    }

                    if (player != null)
                    {
                        if (enableGapCalculations == true)
                        {
                            team.GapToPlayer = GetGap(team, player, StatusDatabase.Opponents.GetUnique(team, Game)?.GaptoPlayer);
                            team.RelativeGapToPlayer = GetRelativeGap(team, player, StatusDatabase.Opponents.GetUnique(team, Game)?.RelativeGapToPlayer);
                        }

                        team.GapToPlayerString = GetGapAsString(team, player, team.GapToPlayer, _telemetry.EnableInverseGapStrings);
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
                    }
                });
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
            }

            foreach (var carClass in CarClasses)
            {
                foreach (var team in carClass.Teams)
                {
                    team.LivePositionInClass = carClass.Teams.Count(x => x.LivePosition <= team.LivePosition);
                }
            }
        }

        private void CreateSimHubActions()
        {
            _plugin.AddAction("ResetBestLaps", ResetBestLaps);
        }

        private void CreateSimHubProperties()
        {
            _plugin.AddProperty("Player_Incidents", 0);

            _plugin.AttachDelegate("Game_IsSupported", () => Game.IsSupported?.ToString() ?? "Untested");
            _plugin.AttachDelegate("Game_Name", () => Game.Name);
            _plugin.AttachDelegate("Session_Description", () => Description);
            _plugin.AttachDelegate("Session_IsMultiClass", () => IsMultiClass);
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
                    carClass = new CarClass(CarClasses.Count() + 1, _plugin)
                    {
                        Color = opponent.CarClassColor,
                        Name = opponent.CarClass,
                        TextColor = opponent.CarClassTextColor,
                    };

                    CarClasses.Add(carClass);
                }

                var team = carClass.Teams.GetUnique(opponent, Game);
                if (team == null)
                {
                    team = new Team(CarClasses.SelectMany(x => x.Teams).Count() + 1, _plugin, carClass, _telemetry)
                    {
                        CarNumber = opponent.CarNumber,
                        CurrentLap = new Lap(opponent.CurrentLap ?? 0)
                        {
                            IsOutLap = opponent.IsCarInPitLane,
                        },
                        CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                        IsInPit = opponent.IsCarInPitLane,
                        LeaderboardPosition = i + 1,
                        Name = opponent.TeamName,
                    };

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
                foreach (CarClass carClass in e.OldItems)
                {
                    carClass.Teams.CollectionChanged -= OnTeamsCollectionChanged;
                }
            }

            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (CarClass carClass in e.NewItems)
                {
                    carClass.Teams.CollectionChanged += OnTeamsCollectionChanged;
                }
            }
        }

        private void OnDescriptionChanging()
        {
            if (Description != null)
            {
                RequestNew?.Invoke(this, System.EventArgs.Empty);
            }
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
                        UpdateLeaderboardPosition();
                    }

                    // 0, 6, 12, 18, 24...
                    if (_counter % 6 == 0)
                    {
                        CalculateGaps(_telemetry.EnableGapCalculations);
                    }

                    // 2, 8, 14, 20, 26...
                    if (_counter % 6 == 2)
                    {
                        CalculateDeltas();
                    }

                    // 4, 10, 16, 22, 28...
                    if (_counter % 6 == 4)
                    {
                        CalculateEstimatedLapTimes(_telemetry.ReferenceLap);
                    }

                    // 30
                    if (_counter % 60 == 30 && Game.IsIRacing == true)
                    {
                        CalculateIRating();
                    }
                }
            }
            else
            {
                RequestNew?.Invoke(this, System.EventArgs.Empty);
            }
        }

        private void OnTeamsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int teamsCount = CarClasses.SelectMany(x => x.Teams).Count();

            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                for (int i = teamsCount + 1; i <= teamsCount + e.OldItems.Count; i++)
                {
                    _plugin.DetachDelegate($"LeaderboardPosition_{i:D2}_Team");
                    _plugin.DetachDelegate($"LivePosition_{i:D2}_Team");
                }
            }

            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                for (int i = teamsCount - e.NewItems.Count + 1; i <= teamsCount; i++)
                {
                    int j = i;

                    _plugin.AttachDelegate($"LeaderboardPosition_{i:D2}_Team", () => CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.LeaderboardPosition == j)?.Index);
                    _plugin.AttachDelegate($"LivePosition_{i:D2}_Team", () => CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.LivePosition == j)?.Index);
                }
            }
        }

        private void ResetBestLaps(PluginManager _, string __)
        {
            foreach (var driver in CarClasses.SelectMany(x => x.Teams).SelectMany(x => x.Drivers))
            {
                driver.BestLap = null;
            }
        }

        private void UpdateLeaderboardPosition()
        {
            foreach (var (opponent, i) in StatusDatabase.Opponents.Select((opponent, i) => (opponent, i)))
            {
                var team = CarClasses.SelectMany(x => x.Teams).GetUnique(opponent, Game);
                if (team != null)
                {
                    team.LeaderboardPosition = i + 1;
                }
            }

            var absentTeams = CarClasses.SelectMany(x => x.Teams).GetAbsent(StatusDatabase.Opponents, Game).ToList();
            foreach (var team in absentTeams)
            {
                team.LeaderboardPosition = -1;
            }
        }
    }
}