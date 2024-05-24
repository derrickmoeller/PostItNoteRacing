using GameReaderCommon;
using IRacingReader;
using PostItNoteRacing.Plugin.EventArgs;
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
        private readonly IModifySimHub _plugin;
        private readonly TelemetryViewModel _telemetry;
        private readonly double _weight = 1600 / Math.Log(2);

        private ObservableCollection<CarClass> _carClasses;
        private short _counter;
        private string _description;

        public Session(IModifySimHub plugin, TelemetryViewModel telemetry)
        {
            _plugin = plugin;
            _plugin.DataUpdated += OnPluginDataUpdated;

            _telemetry = telemetry;

            CreateSimHubActions();
            CreateSimHubProperties();
        }

        public StatusDataBase StatusDatabase { get; set; }

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
                if (_description != value)
                {
                    _description = value;
                    OnDescriptionChanged();
                }
            }
        }

        private bool IsMultiClass => CarClasses.Count > 1;

        private bool IsPractice
        {
            get
            {
                switch (Description.ToUpper())
                {
                    case "OFFLINE TESTING":
                    case "OPEN PRACTICE":
                    case "PRACTICE":
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
                switch (Description.ToUpper())
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
                switch (Description.ToUpper())
                {
                    case "RACE":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public void CalculateDeltas()
        {
            var leader = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.LivePosition == 1);
            var player = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            foreach (var carClass in CarClasses)
            {
                var classLeader = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == 1);

                foreach (var team in carClass.Teams)
                {
                    team.DeltaToBest = (team.BestLap?.Time - carClass.Teams.Where(x => x.BestLap?.Time > TimeSpan.Zero).Min(x => x.BestLap?.Time))?.TotalSeconds ?? 0D;
                    team.DeltaToBestN = (team.BestNLapsAverage - carClass.Teams.Where(x => x.BestNLapsAverage > TimeSpan.Zero).Min(x => x.BestNLapsAverage))?.TotalSeconds ?? 0D;

                    if (leader != null)
                    {
                        team.GapToLeaderString = GetGapAsString(team, leader, team.GapToLeader);
                    }

                    if (classLeader != null)
                    {
                        team.GapToClassLeaderString = GetGapAsString(team, classLeader, team.GapToClassLeader);
                    }

                    if (player != null)
                    {
                        team.DeltaToPlayerBest = (team.BestLap?.Time - player.BestLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerBestN = (team.BestNLapsAverage - player.BestNLapsAverage)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLast = (team.LastLap?.Time - player.LastLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLastN = (team.LastNLapsAverage - player.LastNLapsAverage)?.TotalSeconds ?? 0D;
                        team.GapToPlayerString = GetGapAsString(team, player, team.GapToPlayer);
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
            }

            string GetGapAsString(Team a, Team b, double? gap)
            {
                var laps = a.CurrentLapHighPrecision - b.CurrentLapHighPrecision;
                if (laps > 1 || laps < -1)
                {
                    return _telemetry.EnableInverseGapStrings == true ? $"{laps:-0:+0}L" : $"{laps:+0;-0}L";
                }
                else
                {
                    if (gap != 0)
                    {
                        return _telemetry.EnableInverseGapStrings == true ? $"{gap:-0.0;+0.0}" : $"{gap:+0.0;-0.0}";
                    }
                    else
                    {
                        return "0.0";
                    }
                }
            }

            string GetIntervalAsString(Team a, Team b, double? interval)
            {
                var laps = b.CurrentLapHighPrecision - a.CurrentLapHighPrecision;
                if (laps > 1)
                {
                    return $"{laps:+0}L";
                }
                else
                {
                    return $"{interval:+0.0;-0.0}";
                }
            }
        }

        public void CalculateEstimatedLapTimes(ReferenceLap referenceLap)
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

        public void CalculateGaps()
        {
            var leader = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.LivePosition == 1);
            var player = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            Parallel.ForEach(CarClasses, carClass =>
            {
                var classLeader = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == 1);

                foreach (var team in carClass.Teams.OrderBy(x => x.LivePositionInClass))
                {
                    if (leader != null)
                    {
                        team.GapToLeader = StatusDatabase.Opponents.SingleOrDefault(x => int.Parse(x.CarNumber) == team.CarNumber)?.GaptoLeader ?? 0D;

                        var lap = team.ActiveDriver?.BestLap;
                        if (lap == null && team.LastLap?.IsInLap == false && team.LastLap?.IsOutLap == false && team.LastLap?.Time > TimeSpan.Zero)
                        {
                            lap = team.LastLap;
                        }

                        if (lap != null)
                        {
                            double gapToLeader = 0D;

                            var laps = leader.CurrentLapHighPrecision - team.CurrentLapHighPrecision;
                            if (laps > 1)
                            {
                                gapToLeader = (int)laps * lap.Time.TotalSeconds;
                            }

                            if (team.CurrentLap.MiniSectors.Any() && leader.CurrentLap.MiniSectors.Any())
                            {
                                var leaderMiniSector = GetInterpolatedMiniSector(leader.CurrentLap.LastMiniSector.TrackPosition, lap);
                                var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);

                                if (leaderMiniSector.TrackPosition > teamMiniSector.TrackPosition)
                                {
                                    gapToLeader += (leaderMiniSector.Time - teamMiniSector.Time).TotalSeconds;
                                }
                                else if (leaderMiniSector.TrackPosition < teamMiniSector.TrackPosition)
                                {
                                    gapToLeader += (lap.Time - (teamMiniSector.Time - leaderMiniSector.Time)).TotalSeconds;
                                }
                            }

                            team.GapToLeader = gapToLeader;
                        }
                    }

                    if (classLeader != null)
                    {
                        team.GapToClassLeader = StatusDatabase.Opponents.SingleOrDefault(x => int.Parse(x.CarNumber) == team.CarNumber)?.GaptoClassLeader ?? 0D;

                        var lap = team.ActiveDriver?.BestLap;
                        if (lap == null && team.LastLap?.IsInLap == false && team.LastLap?.IsOutLap == false && team.LastLap?.Time > TimeSpan.Zero)
                        {
                            lap = team.LastLap;
                        }

                        if (lap != null)
                        {
                            double gapToClassLeader = 0D;

                            var laps = classLeader.CurrentLapHighPrecision - team.CurrentLapHighPrecision;
                            if (laps > 1)
                            {
                                gapToClassLeader = (int)laps * lap.Time.TotalSeconds;
                            }

                            if (team.CurrentLap.MiniSectors.Any() && classLeader.CurrentLap.MiniSectors.Any())
                            {
                                var classLeaderMiniSector = GetInterpolatedMiniSector(classLeader.CurrentLap.LastMiniSector.TrackPosition, lap);
                                var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);

                                if (classLeaderMiniSector.TrackPosition > teamMiniSector.TrackPosition)
                                {
                                    gapToClassLeader += (classLeaderMiniSector.Time - teamMiniSector.Time).TotalSeconds;
                                }
                                else if (classLeaderMiniSector.TrackPosition < teamMiniSector.TrackPosition)
                                {
                                    gapToClassLeader += (lap.Time - (teamMiniSector.Time - classLeaderMiniSector.Time)).TotalSeconds;
                                }
                            }

                            team.GapToClassLeader = gapToClassLeader;
                        }
                    }

                    if (player != null)
                    {
                        team.GapToPlayer = StatusDatabase.Opponents.SingleOrDefault(x => int.Parse(x.CarNumber) == team.CarNumber)?.GaptoPlayer ?? 0D;
                        team.RelativeGapToPlayer = StatusDatabase.Opponents.SingleOrDefault(x => int.Parse(x.CarNumber) == team.CarNumber)?.RelativeGapToPlayer;

                        if (player.CurrentLapHighPrecision > team.CurrentLapHighPrecision)
                        {
                            var lap = team.ActiveDriver?.BestLap;
                            if (lap == null && team.LastLap?.IsInLap == false && team.LastLap?.IsOutLap == false && team.LastLap?.Time > TimeSpan.Zero)
                            {
                                lap = team.LastLap;
                            }

                            if (lap != null)
                            {
                                double gapToPlayer = 0D;

                                var laps = player.CurrentLapHighPrecision - team.CurrentLapHighPrecision;
                                if (laps > 1)
                                {
                                    gapToPlayer = (int)laps * lap.Time.TotalSeconds;
                                }

                                if (team.CurrentLap.MiniSectors.Any() && player.CurrentLap.MiniSectors.Any())
                                {
                                    var playerMiniSector = GetInterpolatedMiniSector(player.CurrentLap.LastMiniSector.TrackPosition, lap);
                                    var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);

                                    if (playerMiniSector.TrackPosition > teamMiniSector.TrackPosition)
                                    {
                                        gapToPlayer += (playerMiniSector.Time - teamMiniSector.Time).TotalSeconds;
                                    }
                                    else if (playerMiniSector.TrackPosition < teamMiniSector.TrackPosition)
                                    {
                                        gapToPlayer += (lap.Time - (teamMiniSector.Time - playerMiniSector.Time)).TotalSeconds;
                                    }
                                }

                                team.GapToPlayer = gapToPlayer;
                            }
                        }
                        else if (player.CurrentLapHighPrecision < team.CurrentLapHighPrecision)
                        {
                            var lap = player.ActiveDriver?.BestLap;
                            if (lap == null && player.LastLap?.IsInLap == false && player.LastLap?.IsOutLap == false && player.LastLap?.Time > TimeSpan.Zero)
                            {
                                lap = player.LastLap;
                            }

                            if (lap != null)
                            {
                                double gapToPlayer = 0D;

                                var laps = player.CurrentLapHighPrecision - team.CurrentLapHighPrecision;
                                if (laps < -1)
                                {
                                    gapToPlayer = (int)laps * lap.Time.TotalSeconds;
                                }

                                if (team.CurrentLap.MiniSectors.Any() && player.CurrentLap.MiniSectors.Any())
                                {
                                    var playerMiniSector = GetInterpolatedMiniSector(player.CurrentLap.LastMiniSector.TrackPosition, lap);
                                    var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);

                                    if (teamMiniSector.TrackPosition > playerMiniSector.TrackPosition)
                                    {
                                        gapToPlayer -= (teamMiniSector.Time - playerMiniSector.Time).TotalSeconds;
                                    }
                                    else if (teamMiniSector.TrackPosition < playerMiniSector.TrackPosition)
                                    {
                                        gapToPlayer -= (lap.Time - (playerMiniSector.Time - teamMiniSector.Time)).TotalSeconds;
                                    }
                                }

                                team.GapToPlayer = gapToPlayer;
                            }
                        }
                        else if (player.CurrentLapHighPrecision == team.CurrentLapHighPrecision)
                        {
                            team.GapToPlayer = 0D;
                        }

                        if (team.RelativeGapToPlayer > 0)
                        {
                            var lap = team.ActiveDriver?.BestLap;
                            if (lap == null && team.LastLap?.IsInLap == false && team.LastLap?.IsOutLap == false && team.LastLap?.Time > TimeSpan.Zero)
                            {
                                lap = team.LastLap;
                            }

                            if (lap != null)
                            {
                                if (team.CurrentLap.MiniSectors.Any() && player.CurrentLap.MiniSectors.Any())
                                {
                                    var playerMiniSector = GetInterpolatedMiniSector(player.CurrentLap.LastMiniSector.TrackPosition, lap);
                                    var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);

                                    if (playerMiniSector.TrackPosition > teamMiniSector.TrackPosition)
                                    {
                                        team.RelativeGapToPlayer = (playerMiniSector.Time - teamMiniSector.Time).TotalSeconds;
                                    }
                                    else if (playerMiniSector.TrackPosition < teamMiniSector.TrackPosition)
                                    {
                                        team.RelativeGapToPlayer = (lap.Time - (teamMiniSector.Time - playerMiniSector.Time)).TotalSeconds;
                                    }
                                }
                            }
                        }
                        else if (team.RelativeGapToPlayer < 0)
                        {
                            var lap = player.ActiveDriver?.BestLap;
                            if (lap == null && player.LastLap?.IsInLap == false && player.LastLap?.IsOutLap == false && player.LastLap?.Time > TimeSpan.Zero)
                            {
                                lap = player.LastLap;
                            }

                            if (lap != null)
                            {
                                if (team.CurrentLap.MiniSectors.Any() && player.CurrentLap.MiniSectors.Any())
                                {
                                    var playerMiniSector = GetInterpolatedMiniSector(player.CurrentLap.LastMiniSector.TrackPosition, lap);
                                    var teamMiniSector = GetInterpolatedMiniSector(team.CurrentLap.LastMiniSector.TrackPosition, lap);

                                    if (teamMiniSector.TrackPosition > playerMiniSector.TrackPosition)
                                    {
                                        team.RelativeGapToPlayer = -(teamMiniSector.Time - playerMiniSector.Time).TotalSeconds;
                                    }
                                    else if (teamMiniSector.TrackPosition < playerMiniSector.TrackPosition)
                                    {
                                        team.RelativeGapToPlayer = -(lap.Time - (playerMiniSector.Time - teamMiniSector.Time)).TotalSeconds;
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        public void CalculateIRating()
        {
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

            double GetIRatingChange(int teamIRating, int position, IEnumerable<int> iRatings)
            {
                double factor = ((iRatings.Count() / 2D) - position) / 100D;
                double sum = -0.5;

                foreach (var iRating in iRatings)
                {
                    sum += (1 - Math.Exp(-teamIRating / _weight)) * Math.Exp(-iRating / _weight) / (((1 - Math.Exp(-iRating / _weight)) * Math.Exp(-teamIRating / _weight)) + ((1 - Math.Exp(-teamIRating / _weight)) * Math.Exp(-iRating / _weight)));
                }

                return Math.Round((iRatings.Count() - position - sum - factor) * 200 / iRatings.Count());
            }
        }

        public void CalculateLivePositions()
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

        public void GenerateMiniSectors()
        {
            Parallel.ForEach(StatusDatabase.Opponents, opponent =>
            {
                var team = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.CarNumber == int.Parse(opponent.CarNumber));
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

        public void GetGameData()
        {
            Description = StatusDatabase.SessionTypeName;

            foreach (var opponent in StatusDatabase.Opponents)
            {
                var carClass = CarClasses.SingleOrDefault(x => x.Color == opponent.CarClassColor);
                if (carClass == null)
                {
                    carClass = new CarClass
                    {
                        Color = opponent.CarClassColor,
                        Name = opponent.CarClass,
                        TextColor = opponent.CarClassTextColor,
                    };

                    CarClasses.Add(carClass);
                }

                var team = carClass.Teams.SingleOrDefault(x => x.CarNumber == int.Parse(opponent.CarNumber));
                if (team == null)
                {
                    team = new Team(carClass, _telemetry)
                    {
                        CarNumber = int.Parse(opponent.CarNumber),
                        CurrentLap = new Lap(opponent.CurrentLap.Value)
                        {
                            IsOutLap = opponent.IsCarInPitLane,
                        },
                        CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                        IsInPit = opponent.IsCarInPitLane,
                        Name = opponent.TeamName,
                    };

                    _plugin.AttachDelegate($"Drivers_Car_{team.CarNumber:D3}_LeaderboardPosition", () => { return team.LeaderboardPosition; });

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
                    team.BestLap = new Lap(opponent.CurrentLap - 1 ?? 0);

                    team.BestLap.MiniSectors.Add(new MiniSector
                    {
                        Time = opponent.BestLapTime,
                        TrackPosition = 1,
                    });
                }

                if (IsRace == true && StatusDatabase.Flag_Green == 1)
                {
                    team.GridPosition = CarClasses.SelectMany(x => x.Teams).Count(x => x.LeaderboardPosition <= team.LeaderboardPosition);
                    team.GridPositionInClass = carClass.Teams.Count(x => x.LeaderboardPosition <= team.LeaderboardPosition);
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

        public void Reset()
        {
            CarClasses.Clear();

            for (int i = 1; i <= 63; i++)
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

            for (int i = 1; i <= CarClass.Colors.Count; i++)
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

        public void WriteSimHubData()
        {
            foreach (var (opponent, i) in StatusDatabase.Opponents.Select((opponent, i) => (opponent, i)))
            {
                var team = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.CarNumber == int.Parse(opponent.CarNumber));
                if (team != null)
                {
                    team.LeaderboardPosition = i + 1;
                }
            }

            Parallel.ForEach(CarClasses, carClass =>
            {
                _plugin.SetProperty($"Class_{carClass.Index:D2}_OpponentCount", carClass.Teams.Count);

                Parallel.ForEach(carClass.Teams, team =>
                {
                    _plugin.SetProperty($"Class_{carClass.Index:D2}_{team.LivePositionInClass:D2}_LeaderboardPosition", team.LeaderboardPosition);

                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestNLapsAverage", team.BestNLapsAverage ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestNLapsColor", team.BestNLapsColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapColor", team.ActiveDriver.BestLapColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapTime", team.ActiveDriver.BestLap?.Time ?? TimeSpan.Zero);
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
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LapsCompleted", team.ActiveDriver.LapsCompleted);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastNLapsAverage", team.LastNLapsAverage ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastNLapsColor", team.LastNLapsColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapColor", team.LastLapColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapTime", team.LastLap?.Time ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePosition", team.LivePosition);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePositionInClass", team.LivePositionInClass);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_PositionsGained", team.PositionsGained);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_PositionsGainedInClass", team.PositionsGainedInClass);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayer", team.RelativeGapToPlayer);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerColor", IsRace ? team.RelativeGapToPlayerColor : Colors.White);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerString", team.RelativeGapToPlayerString);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapColor", team.BestLapColor);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapTime", team.BestLap?.Time ?? TimeSpan.Zero);
                    _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamLapsCompleted", team.LapsCompleted);

                    _plugin.SetProperty($"Drivers_Live_{team.LivePosition:D2}_LeaderboardPosition", team.LeaderboardPosition);

                    if (team.IsDirty == true)
                    {
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CarNumber", team.CarNumber);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassColor", carClass.Color);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassIndex", carClass.Index);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassString", carClass.Name);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassTextColor", carClass.TextColor);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsPlayer", team.IsPlayer);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LeaderboardPosition", team.LeaderboardPosition);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseColor", team.ActiveDriver.License.Color);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseShortString", team.ActiveDriver.License.ShortString);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseString", team.ActiveDriver.License.String);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseTextColor", team.ActiveDriver.License.TextColor);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_Name", team.ActiveDriver.Name);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ShortName", team.ActiveDriver.ShortName);
                        _plugin.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamName", team.Name);

                        team.IsDirty = false;
                    }
                });
            });
        }

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

            long GetLinearInterpolation(double x, double x0, double x1, long y0, long y1)
            {
                if ((x1 - x0) == 0)
                {
                    return (y0 + y1) / 2;
                }

                return (long)(y0 + ((x - x0) * (y1 - y0) / (x1 - x0)));
            }
        }

        private void CreateSimHubActions()
        {
            _plugin.AddAction("DecrementNLaps", (a, b) => _telemetry.NLaps--);
            _plugin.AddAction("IncrementNLaps", (a, b) => _telemetry.NLaps++);
            _plugin.AddAction("LastReferenceLap", (a, b) => _telemetry.ReferenceLap--);
            _plugin.AddAction("NextReferenceLap", (a, b) => _telemetry.ReferenceLap++);
            _plugin.AddAction("ResetBestLaps", ResetBestLaps);
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

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                _plugin.AddProperty($"Class_{i:D2}_OpponentCount", 0);
                _plugin.AddProperty($"Class_{i:D2}_SoF", 0);
                _plugin.AddProperty($"Class_{i:D2}_SoFString", string.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    _plugin.AddProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            _plugin.AddProperty("Player_Incidents", 0);
            _plugin.AddProperty("Session_Description", string.Empty);
            _plugin.AddProperty("Session_IsMultiClass", false);
            _plugin.AttachDelegate("Settings_NLaps", () => _telemetry.NLaps);
            _plugin.AttachDelegate("Settings_OverrideJavaScriptFunctions", () => _telemetry.OverrideJavaScriptFunctions);
            _plugin.AttachDelegate("Settings_ReferenceLap", () => _telemetry.ReferenceLap);
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
            Reset();

            _plugin.SetProperty("Session_Description", Description);
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
                if (_counter % 6 == 0 && _telemetry.EnableGapCalculations)
                {
                    CalculateGaps();
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
                if (_counter % 60 == 30 && e.Data.GameName == "IRacing")
                {
                    CalculateIRating();
                }
            }
            else
            {
                Reset();
            }
        }

        private void ResetBestLaps(PluginManager _, string __)
        {
            foreach (var driver in CarClasses.SelectMany(x => x.Teams).SelectMany(x => x.Drivers))
            {
                driver.BestLap = null;
            }
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