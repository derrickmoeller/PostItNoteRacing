using GameReaderCommon;
using IRacingReader;
using PostItNoteRacing.Plugin.Extensions;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.ViewModels;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Telemetry : IDisposable
    {
        private readonly IModifySimHub _modifySimHub;
        private readonly SettingsViewModel _settings;
        private readonly double _weight = 1600 / Math.Log(2);

        private ObservableCollection<CarClass> _carClasses;
        private string _description;

        public Telemetry(IModifySimHub modifySimHub, SettingsViewModel settings)
        {
            _modifySimHub = modifySimHub;
            _settings = settings;

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
            var player = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            foreach (var carClass in CarClasses)
            {
                var leader = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == 1);

                foreach (var team in carClass.Teams)
                {
                    team.DeltaToBest = (team.BestLapTime - carClass.Teams.Where(x => x.BestLapTime > TimeSpan.Zero).Min(x => x.BestLapTime))?.TotalSeconds ?? 0D;
                    team.DeltaToBestN = (team.BestNLapsAverage - carClass.Teams.Where(x => x.BestNLapsAverage > TimeSpan.Zero).Min(x => x.BestNLapsAverage))?.TotalSeconds ?? 0D;

                    if (leader != null)
                    {
                        team.GapToLeaderString = GetGapAsString(team, leader, team.GapToLeader);
                    }

                    if (player != null)
                    {
                        team.DeltaToPlayerBest = (team.BestLapTime - player.BestLapTime)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerBestN = (team.BestNLapsAverage - player.BestNLapsAverage)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLast = (team.LastLap?.Time - player.LastLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLastN = (team.LastNLapsAverage - player.LastNLapsAverage)?.TotalSeconds ?? 0D;
                        team.GapToPlayerString = GetGapAsString(team, player, team.GapToPlayer);
                    }

                    if (team.LivePositionInClass == 1)
                    {
                        team.Interval = 0d;
                        team.IntervalString = $"L{team.LapsCompleted + 1}";
                    }
                    else
                    {
                        var teamAhead = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == team.LivePositionInClass - 1);
                        if (teamAhead != null)
                        {
                            team.Interval = team.GapToLeader - teamAhead.GapToLeader;
                            team.IntervalString = GetIntervalAsString(team, teamAhead, team.Interval);
                        }
                    }
                }
            }

            string GetGapAsString(Team a, Team b, double? gap)
            {
                var laps = a.CurrentLapHighPrecision - b.CurrentLapHighPrecision;
                if (laps > 1)
                {
                    return $"+{(int)laps}L";
                }
                else if (laps < -1)
                {
                    return $"{(int)laps}L";
                }
                else
                {
                    if (gap != 0)
                    {
                        return $"{gap:-0.0;+0.0}";
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
                    return $"+{(int)laps}L";
                }
                else if (laps < -1)
                {
                    return $"{(int)laps}L";
                }
                else
                {
                    return $"{interval:+0.0;-0.0}";
                }
            }
        }

        public void CalculateEstimatedLapTimes()
        {
            Parallel.ForEach(CarClasses.SelectMany(x => x.Teams), team =>
            {
                var bestLap = team.Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap;

                if (bestLap != null && team.CurrentLap.MiniSectors.Any())
                {
                    var miniSector = team.CurrentLap.LastMiniSector;

                    team.EstimatedLapTime = bestLap.Time + (miniSector.Time - GetInterpolatedMiniSector(miniSector.TrackPosition, bestLap).Time);
                }
                else
                {
                    team.EstimatedLapTime = null;
                }
            });
        }

        public void CalculateGaps()
        {
            var player = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            Parallel.ForEach(CarClasses, carClass =>
            {
                var leader = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == 1);

                foreach (var team in carClass.Teams.OrderBy(x => x.LivePositionInClass))
                {
                    if (leader != null)
                    {
                        team.GapToLeader = StatusDatabase.Opponents.SingleOrDefault(x => x.CarNumber == team.CarNumber)?.GaptoClassLeader ?? 0D;

                        var lap = team.Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap;
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

                    if (player != null)
                    {
                        team.GapToPlayer = StatusDatabase.Opponents.SingleOrDefault(x => x.CarNumber == team.CarNumber)?.GaptoPlayer ?? 0D;
                        team.RelativeGapToPlayer = StatusDatabase.Opponents.SingleOrDefault(x => x.CarNumber == team.CarNumber)?.RelativeGapToPlayer;

                        if (player.CurrentLapHighPrecision > team.CurrentLapHighPrecision)
                        {
                            var lap = team.Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap;
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
                            var lap = player.Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap;
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
                            var lap = team.Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap;
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
                            var lap = player.Drivers.SingleOrDefault(x => x.IsActive == true)?.BestLap;
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

                        _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingChange", team.Drivers.Single(x => x.IsActive).IRatingChange);
                    }

                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRating", team.Drivers.Single(x => x.IsActive).IRating);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingLicenseCombinedString", team.Drivers.Single(x => x.IsActive).IRatingLicenseCombinedString);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingString", team.Drivers.Single(x => x.IsActive).IRatingString);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamIRating", team.IRating);
                });

                _modifySimHub.SetProperty($"Class_{carClass.Index:D2}_SoF", carClass.StrengthOfField);
                _modifySimHub.SetProperty($"Class_{carClass.Index:D2}_SoFString", carClass.StrengthOfFieldString);
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
                var team = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.CarNumber == opponent.CarNumber);
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

                var team = carClass.Teams.SingleOrDefault(x => x.CarNumber == opponent.CarNumber);
                if (team == null)
                {
                    team = new Team(carClass, _settings)
                    {
                        CarNumber = opponent.CarNumber,
                        CurrentLap = new Lap(opponent.CurrentLap.Value)
                        {
                            IsOutLap = opponent.IsCarInPitLane,
                        },
                        CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                        IsInPit = opponent.IsCarInPitLane,
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

                team.Drivers.ForEach(x => x.IsActive = false);
                team.IsConnected = opponent.IsConnected;
                team.IsPlayer = opponent.IsPlayer;

                if (IsQualifying == true && opponent.BestLapTime > TimeSpan.Zero)
                {
                    team.BestLapTime = opponent.BestLapTime;
                }

                if (_settings.EnableGapCalculations == false)
                {
                    team.GapToLeader = opponent.GaptoClassLeader ?? 0D;
                    team.GapToPlayer = opponent.GaptoPlayer ?? 0D;
                    team.RelativeGapToPlayer = opponent.IsConnected ? opponent.RelativeGapToPlayer : null;
                }

                var driver = team.Drivers.SingleOrDefault(x => x.Name == opponent.Name);
                if (driver == null)
                {
                    driver = new Driver(carClass, team.IsPlayer)
                    {
                        IRating = opponent.IRacing_IRating,
                        IsActive = true,
                        License = new License
                        {
                            String = opponent.LicenceString,
                        },
                        Name = opponent.Name,
                    };

                    team.Drivers.Add(driver);
                }
                else
                {
                    driver.IsActive = true;
                }
            }

            if (StatusDatabase.GetRawDataObject() is DataSampleEx iRacingData)
            {
                iRacingData.Telemetry.TryGetValue("PlayerCarTeamIncidentCount", out object rawIncidents);

                _modifySimHub.SetProperty("Player_Incidents", Convert.ToInt32(rawIncidents));
            }
        }

        public void Reset()
        {
            CarClasses.Clear();

            for (int i = 1; i <= 63; i++)
            {
                _modifySimHub.SetProperty($"Drivers_{i:D2}_BestNLapsAverage", TimeSpan.Zero);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_BestNLapsColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_BestLapColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_CarNumber", -1);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_ClassColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_ClassIndex", -1);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_ClassString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_ClassTextColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_CurrentLapTime", TimeSpan.Zero);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_DeltaToBestN", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_DeltaToPlayerBestN", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_DeltaToPlayerLastN", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_EstimatedDelta", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_EstimatedLapColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_EstimatedLapTime", TimeSpan.Zero);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_GapToLeader", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_GapToLeaderString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_GapToPlayerString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_Interval", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IntervalString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IRating", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IRatingChange", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IRatingString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IsConnected", false);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IsInPit", false);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_IsPlayer", false);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LastNLapsAverage", TimeSpan.Zero);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LastNLapsColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LastLapColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LicenseColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LicenseShortString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LicenseString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LicenseTextColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LivePosition", -1);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_Name", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_ShortName", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_TeamBestLapColor", string.Empty);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_TeamBestLapTime", TimeSpan.Zero);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_TeamIRating", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                _modifySimHub.SetProperty($"Drivers_{i:D2}_TeamName", string.Empty);

                _modifySimHub.SetProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                _modifySimHub.SetProperty($"Class_{i:D2}_OpponentCount", 0);
                _modifySimHub.SetProperty($"Class_{i:D2}_SoF", 0);
                _modifySimHub.SetProperty($"Class_{i:D2}_SoFString", string.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    _modifySimHub.SetProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            _modifySimHub.SetProperty("Player_Incidents", 0);
            _modifySimHub.SetProperty("Session_Description", string.Empty);
            _modifySimHub.SetProperty("Session_IsMultiClass", false);
        }

        public void WriteSimHubData()
        {
            foreach (var (opponent, i) in StatusDatabase.Opponents.Select((opponent, i) => (opponent, i)))
            {
                var team = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.CarNumber == opponent.CarNumber);
                if (team != null)
                {
                    team.LeaderboardPosition = i + 1;
                }
            }

            Parallel.ForEach(CarClasses, carClass =>
            {
                _modifySimHub.SetProperty($"Class_{carClass.Index:D2}_OpponentCount", carClass.Teams.Count);

                Parallel.ForEach(carClass.Teams, team =>
                {
                    _modifySimHub.SetProperty($"Class_{carClass.Index:D2}_{team.LivePositionInClass:D2}_LeaderboardPosition", team.LeaderboardPosition);

                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestNLapsAverage", team.BestNLapsAverage ?? TimeSpan.Zero);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestNLapsColor", team.BestNLapsColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapColor", team.Drivers.Single(x => x.IsActive == true).BestLapColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapTime", team.Drivers.Single(x => x.IsActive == true).BestLap?.Time ?? TimeSpan.Zero);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CarNumber", team.CarNumber);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassColor", carClass.Color);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassIndex", carClass.Index);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassString", carClass.Name);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassTextColor", carClass.TextColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapHighPrecision", team.CurrentLapHighPrecision);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapTime", team.CurrentLap.Time);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBest", team.DeltaToBest);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBestN", team.DeltaToBestN);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBest", team.DeltaToPlayerBest);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBestN", team.DeltaToPlayerBestN);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLast", team.DeltaToPlayerLast);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLastN", team.DeltaToPlayerLastN);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedDelta", team.EstimatedDelta);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapColor", team.EstimatedLapColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapTime", team.EstimatedLapTime ?? TimeSpan.Zero);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeader", team.GapToLeader);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeaderString", team.GapToLeaderString);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayer", team.GapToPlayer);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayerString", team.GapToPlayerString);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_Interval", team.Interval);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalString", team.IntervalString);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsConnected", team.IsConnected);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsInPit", team.IsInPit);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_IsPlayer", team.IsPlayer);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LapsCompleted", team.Drivers.Single(x => x.IsActive == true).LapsCompleted);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastNLapsAverage", team.LastNLapsAverage ?? TimeSpan.Zero);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastNLapsColor", team.LastNLapsColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapColor", team.LastLapColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapTime", team.LastLap?.Time ?? TimeSpan.Zero);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseColor", team.Drivers.Single(x => x.IsActive == true).License.Color);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseShortString", team.Drivers.Single(x => x.IsActive == true).License.ShortString);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseString", team.Drivers.Single(x => x.IsActive == true).License.String);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseTextColor", team.Drivers.Single(x => x.IsActive == true).License.TextColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePosition", team.LivePosition);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePositionInClass", team.LivePositionInClass);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_Name", team.Drivers.Single(x => x.IsActive == true).Name);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayer", team.RelativeGapToPlayer);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerColor", IsRace ? team.RelativeGapToPlayerColor : Colors.White);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerString", team.RelativeGapToPlayerString);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_ShortName", team.Drivers.Single(x => x.IsActive == true).ShortName);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapColor", team.BestLapColor);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapTime", team.BestLapTime ?? TimeSpan.Zero);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamLapsCompleted", team.LapsCompleted);
                    _modifySimHub.SetProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamName", team.Name);

                    _modifySimHub.SetProperty($"Drivers_Live_{team.LivePosition:D2}_LeaderboardPosition", team.LeaderboardPosition);
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
            _modifySimHub.AddAction("IncrementNLaps", (a, b) => _settings.NLaps++);
            _modifySimHub.AddAction("DecrementNLaps", (a, b) => _settings.NLaps--);
            _modifySimHub.AddAction("ResetBestLaps", ResetBestLaps);
        }

        private void CreateSimHubProperties()
        {
            for (int i = 1; i <= 63; i++)
            {
                _modifySimHub.AddProperty($"Drivers_{i:D2}_BestNLapsAverage", TimeSpan.Zero);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_BestNLapsColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_BestLapColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_CarNumber", -1);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_ClassColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_ClassIndex", -1);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_ClassString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_ClassTextColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_CurrentLapTime", TimeSpan.Zero);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_DeltaToBestN", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_DeltaToPlayerBestN", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_DeltaToPlayerLastN", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_EstimatedDelta", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_EstimatedLapColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_EstimatedLapTime", TimeSpan.Zero);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_GapToLeader", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_GapToLeaderString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_GapToPlayerString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_Interval", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IntervalString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IRating", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IRatingChange", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IRatingString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IsConnected", false);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IsInPit", false);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_IsPlayer", false);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LastNLapsAverage", TimeSpan.Zero);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LastNLapsColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LastLapColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LicenseColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LicenseShortString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LicenseString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LicenseTextColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LivePosition", -1);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_Name", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_ShortName", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_TeamBestLapColor", string.Empty);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_TeamBestLapTime", TimeSpan.Zero);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_TeamIRating", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                _modifySimHub.AddProperty($"Drivers_{i:D2}_TeamName", string.Empty);

                _modifySimHub.AddProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                _modifySimHub.AddProperty($"Class_{i:D2}_OpponentCount", 0);
                _modifySimHub.AddProperty($"Class_{i:D2}_SoF", 0);
                _modifySimHub.AddProperty($"Class_{i:D2}_SoFString", string.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    _modifySimHub.AddProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            _modifySimHub.AddProperty("Player_Incidents", 0);
            _modifySimHub.AddProperty("Session_Description", string.Empty);
            _modifySimHub.AddProperty("Session_IsMultiClass", false);
            _modifySimHub.AttachDelegate("Settings_NLaps", () => _settings.NLaps);
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

            _modifySimHub.SetProperty("Session_IsMultiClass", IsMultiClass);
        }

        private void OnDescriptionChanged()
        {
            Reset();

            _modifySimHub.SetProperty("Session_Description", Description);
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