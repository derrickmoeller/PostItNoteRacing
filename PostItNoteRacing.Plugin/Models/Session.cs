using GameReaderCommon;
using IRacingReader;
using PostItNoteRacing.Plugin.Extensions;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Session : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly Type _pluginType;
        private readonly double _weight = 1600 / Math.Log(2);

        private ObservableCollection<CarClass> _carClasses;
        private string _description;

        public Session(PluginManager pluginManager, Type pluginType)
        {
            _pluginManager = pluginManager;
            _pluginType = pluginType;

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

        public void CalculateGaps()
        {
            var player = CarClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            foreach (var carClass in CarClasses)
            {
                var leader = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == 1);

                foreach (var team in carClass.Teams.OrderBy(x => x.LivePositionInClass))
                {
                    team.DeltaToBest = (team.BestLapTime - carClass.Teams.Where(x => x.BestLapTime > TimeSpan.Zero).Min(x => x.BestLapTime))?.TotalSeconds ?? 0D;
                    team.DeltaToBestFive = (team.BestFiveLapsAverage - carClass.Teams.Where(x => x.BestFiveLapsAverage > TimeSpan.Zero).Min(x => x.BestFiveLapsAverage))?.TotalSeconds ?? 0D;

                    if (leader != null)
                    {
                        team.GapToLeaderString = GetGapAsString(team, leader, team.GapToLeader);
                    }

                    if (player != null)
                    {
                        team.DeltaToPlayerBest = (team.BestLapTime - player.BestLapTime)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerBestFive = (team.BestFiveLapsAverage - player.BestFiveLapsAverage)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLast = (team.LastLap?.Time - player.LastLap?.Time)?.TotalSeconds ?? 0D;
                        team.DeltaToPlayerLastFive = (team.LastFiveLapsAverage - player.LastFiveLapsAverage)?.TotalSeconds ?? 0D;
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

        public void CalculateIRating()
        {
            foreach (var carClass in CarClasses)
            {
                foreach (var team in carClass.Teams.Where(x => x.IRating > 0))
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

                        SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingChange", team.Drivers.Single(x => x.IsActive).IRatingChange);
                    }

                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRating", team.Drivers.Single(x => x.IsActive).IRating);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingLicenseCombinedString", team.Drivers.Single(x => x.IsActive).IRatingLicenseCombinedString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingString", team.Drivers.Single(x => x.IsActive).IRatingString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamIRating", team.IRating);
                }

                SetSimHubProperty($"Class_{carClass.Index:D2}_SoF", carClass.StrengthOfField);
                SetSimHubProperty($"Class_{carClass.Index:D2}_SoFString", carClass.StrengthOfFieldString);
            }

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
            foreach (var carClass in CarClasses)
            {
                foreach (var team in carClass.Teams)
                {
                    if (IsRace == true)
                    {
                        team.LivePosition = CarClasses.SelectMany(x => x.Teams).Count(x => x.CurrentLapHighPrecision > team.CurrentLapHighPrecision) + 1;
                        team.LivePositionInClass = carClass.Teams.Count(x => x.CurrentLapHighPrecision > team.CurrentLapHighPrecision) + 1;
                    }
                    else
                    {
                        team.LivePosition = team.LeaderboardPosition;
                        team.LivePositionInClass = carClass.Teams.Count(x => x.LeaderboardPosition <= team.LeaderboardPosition);
                    }
                }
            }
        }

        public void GetGameData()
        {
            Description = StatusDatabase.SessionTypeName;

            foreach (var (opponent, i) in StatusDatabase.Opponents.Select((opponent, i) => (opponent, i)))
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
                    team = new Team(carClass)
                    {
                        CarNumber = opponent.CarNumber,
                        CurrentLap = new Lap(opponent.CurrentLap.Value)
                        {
                            IsInLap = false,
                            IsOutLap = opponent.IsCarInPitLane,
                            Time = opponent.CurrentLapTime ?? TimeSpan.Zero,
                        },
                        CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                        IsInPit = opponent.IsCarInPitLane,
                        Name = opponent.TeamName,
                        RelativeGapToPlayer = opponent.RelativeGapToPlayer,
                    };

                    carClass.Teams.Add(team);
                }
                else if (opponent.IsConnected == true)
                {
                    team.CurrentLapHighPrecision = opponent.CurrentLapHighPrecision;
                    team.IsInPit = opponent.IsCarInPitLane;
                    team.RelativeGapToPlayer = opponent.RelativeGapToPlayer;
                }
                else if (opponent.IsConnected == false)
                {
                    team.IsInPit = true;
                    team.RelativeGapToPlayer = null;
                }

                team.Drivers.ForEach(x => x.IsActive = false);
                team.GapToLeader = opponent.GaptoClassLeader ?? 0;
                team.GapToPlayer = opponent.GaptoPlayer;
                team.IsConnected = opponent.IsConnected;
                team.IsPlayer = opponent.IsPlayer;

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

                if (IsQualifying == true)
                {
                    if (opponent.BestLapTime > TimeSpan.Zero && opponent.BestLapTime < (team.BestLapTime ?? TimeSpan.MaxValue))
                    {
                        team.BestLapTime = opponent.BestLapTime;
                    }
                }
                else
                {
                    if (opponent.CurrentLap > team.CurrentLap.Number)
                    {
                        team.CurrentLap.IsInLap = opponent.IsCarInPitLane;
                        team.CurrentLap.Time = opponent.LastLapTime;

                        team.CurrentLap = new Lap(opponent.CurrentLap.Value)
                        {
                            IsInLap = false,
                            IsOutLap = opponent.IsCarInPitLane,
                            Time = opponent.CurrentLapTime ?? TimeSpan.Zero,
                        };
                    }
                    else
                    {
                        team.CurrentLap.Time = opponent.CurrentLapTime ?? TimeSpan.Zero;

                        team.CurrentLap.MiniSectors.RemoveAll(x => x.TrackPosition >= opponent.TrackPositionPercent);
                        team.CurrentLap.MiniSectors.Add(new MiniSector
                        {
                            Time = opponent.CurrentLapTime ?? TimeSpan.Zero,
                            TrackPosition = opponent.TrackPositionPercent.Value,
                        });
                    }
                }
            }

            if (StatusDatabase.GetRawDataObject() is DataSampleEx iRacingData)
            {
                iRacingData.Telemetry.TryGetValue("PlayerCarTeamIncidentCount", out object rawIncidents);

                SetSimHubProperty("Player_Incidents", Convert.ToInt32(rawIncidents));
            }
        }

        public void Reset()
        {
            CarClasses.Clear();

            for (int i = 1; i <= 63; i++)
            {
                SetSimHubProperty($"Drivers_{i:D2}_BestFiveLapsAverage", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_BestFiveLapsColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_BestLapColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_CarNumber", -1);
                SetSimHubProperty($"Drivers_{i:D2}_ClassColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_ClassIndex", -1);
                SetSimHubProperty($"Drivers_{i:D2}_ClassString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_ClassTextColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                SetSimHubProperty($"Drivers_{i:D2}_CurrentLapTime", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToBestFive", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerBestFive", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerLastFive", 0);
                SetSimHubProperty($"Drivers_{i:D2}_EstimatedDelta", 0);
                SetSimHubProperty($"Drivers_{i:D2}_EstimatedLapColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_EstimatedLapTime", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_GapToLeader", 0);
                SetSimHubProperty($"Drivers_{i:D2}_GapToLeaderString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                SetSimHubProperty($"Drivers_{i:D2}_GapToPlayerString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_Interval", 0);
                SetSimHubProperty($"Drivers_{i:D2}_IntervalString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_IRating", 0);
                SetSimHubProperty($"Drivers_{i:D2}_IRatingChange", 0);
                SetSimHubProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_IRatingString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_IsConnected", false);
                SetSimHubProperty($"Drivers_{i:D2}_IsInPit", false);
                SetSimHubProperty($"Drivers_{i:D2}_IsPlayer", false);
                SetSimHubProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                SetSimHubProperty($"Drivers_{i:D2}_LastFiveLapsAverage", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_LastFiveLapsColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LastLapColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseShortString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseTextColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LivePosition", -1);
                SetSimHubProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                SetSimHubProperty($"Drivers_{i:D2}_Name", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                SetSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_ShortName", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_TeamBestLapColor", string.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_TeamBestLapTime", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_TeamIRating", 0);
                SetSimHubProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                SetSimHubProperty($"Drivers_{i:D2}_TeamName", string.Empty);
                SetSimHubProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                SetSimHubProperty($"Class_{i:D2}_OpponentCount", 0);
                SetSimHubProperty($"Class_{i:D2}_SoF", 0);
                SetSimHubProperty($"Class_{i:D2}_SoFString", string.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    SetSimHubProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            SetSimHubProperty("Player_Incidents", 0);
            SetSimHubProperty("Session_Description", string.Empty);
            SetSimHubProperty("Session_IsMultiClass", false);
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

            foreach (var carClass in CarClasses)
            {
                SetSimHubProperty($"Class_{carClass.Index:D2}_OpponentCount", carClass.Teams.Count);

                foreach (var team in carClass.Teams)
                {
                    SetSimHubProperty($"Class_{carClass.Index:D2}_{team.LivePositionInClass:D2}_LeaderboardPosition", team.LeaderboardPosition);

                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_BestFiveLapsAverage", team.BestFiveLapsAverage ?? TimeSpan.Zero);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_BestFiveLapsColor", team.BestFiveLapsColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapColor", team.Drivers.Single(x => x.IsActive == true).BestLapColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapTime", team.Drivers.Single(x => x.IsActive == true).BestLap?.Time ?? TimeSpan.Zero);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_CarNumber", team.CarNumber);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassColor", carClass.Color);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassIndex", carClass.Index);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassString", carClass.Name);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassTextColor", carClass.TextColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapHighPrecision", team.CurrentLapHighPrecision);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapTime", team.CurrentLap.Time);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBest", team.DeltaToBest);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBestFive", team.DeltaToBestFive);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBest", team.DeltaToPlayerBest);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBestFive", team.DeltaToPlayerBestFive);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLast", team.DeltaToPlayerLast);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLastFive", team.DeltaToPlayerLastFive);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedDelta", team.EstimatedDelta);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapColor", team.EstimatedLapColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_EstimatedLapTime", team.EstimatedLapTime ?? TimeSpan.Zero);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeader", team.GapToLeader);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeaderString", team.GapToLeaderString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayer", team.GapToPlayer);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayerString", team.GapToPlayerString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_Interval", team.Interval);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalString", team.IntervalString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IsConnected", team.IsConnected);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IsInPit", team.IsInPit);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IsPlayer", team.IsPlayer);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LapsCompleted", team.Drivers.Single(x => x.IsActive == true).LapsCompleted);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LastFiveLapsAverage", team.LastFiveLapsAverage ?? TimeSpan.Zero);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LastFiveLapsColor", team.LastFiveLapsColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapColor", team.LastLapColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapTime", team.LastLap?.Time ?? TimeSpan.Zero);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseColor", team.Drivers.Single(x => x.IsActive == true).License.Color);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseShortString", team.Drivers.Single(x => x.IsActive == true).License.ShortString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseString", team.Drivers.Single(x => x.IsActive == true).License.String);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseTextColor", team.Drivers.Single(x => x.IsActive == true).License.TextColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePosition", team.LivePosition);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePositionInClass", team.LivePositionInClass);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_Name", team.Drivers.Single(x => x.IsActive == true).Name);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayer", team.RelativeGapToPlayer);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerColor", IsRace ? team.RelativeGapToPlayerColor : Colors.White);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerString", team.RelativeGapToPlayerString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ShortName", team.Drivers.Single(x => x.IsActive == true).ShortName);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapColor", team.BestLapColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamBestLapTime", team.BestLapTime ?? TimeSpan.Zero);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamLapsCompleted", team.LapsCompleted);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamName", team.Name);

                    SetSimHubProperty($"Drivers_Live_{team.LivePosition:D2}_LeaderboardPosition", team.LeaderboardPosition);
                }
            }
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

        private void CreateSimHubProperties()
        {
            AddSimHubAction("ResetEstimatedLaps", ResetEstimatedLaps);

            for (int i = 1; i <= 63; i++)
            {
                AddSimHubProperty($"Drivers_{i:D2}_BestFiveLapsAverage", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_BestFiveLapsColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_BestLapColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_CarNumber", -1);
                AddSimHubProperty($"Drivers_{i:D2}_ClassColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_ClassIndex", -1);
                AddSimHubProperty($"Drivers_{i:D2}_ClassString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_ClassTextColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                AddSimHubProperty($"Drivers_{i:D2}_CurrentLapTime", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToBestFive", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerBestFive", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerLastFive", 0);
                AddSimHubProperty($"Drivers_{i:D2}_EstimatedDelta", 0);
                AddSimHubProperty($"Drivers_{i:D2}_EstimatedLapColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_EstimatedLapTime", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_GapToLeader", 0);
                AddSimHubProperty($"Drivers_{i:D2}_GapToLeaderString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                AddSimHubProperty($"Drivers_{i:D2}_GapToPlayerString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_Interval", 0);
                AddSimHubProperty($"Drivers_{i:D2}_IntervalString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_IRating", 0);
                AddSimHubProperty($"Drivers_{i:D2}_IRatingChange", 0);
                AddSimHubProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_IRatingString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_IsConnected", false);
                AddSimHubProperty($"Drivers_{i:D2}_IsInPit", false);
                AddSimHubProperty($"Drivers_{i:D2}_IsPlayer", false);
                AddSimHubProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                AddSimHubProperty($"Drivers_{i:D2}_LastFiveLapsAverage", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_LastFiveLapsColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LastLapColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseShortString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseTextColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LivePosition", -1);
                AddSimHubProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                AddSimHubProperty($"Drivers_{i:D2}_Name", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                AddSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_ShortName", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_TeamBestLapColor", string.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_TeamBestLapTime", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_TeamIRating", 0);
                AddSimHubProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                AddSimHubProperty($"Drivers_{i:D2}_TeamName", string.Empty);
                AddSimHubProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                AddSimHubProperty($"Class_{i:D2}_OpponentCount", 0);
                AddSimHubProperty($"Class_{i:D2}_SoF", 0);
                AddSimHubProperty($"Class_{i:D2}_SoFString", string.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    AddSimHubProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            AddSimHubProperty("Player_Incidents", 0);
            AddSimHubProperty("Session_Description", string.Empty);
            AddSimHubProperty("Session_IsMultiClass", false);
            AddSimHubProperty("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            void AddSimHubAction(string actionName, Action<PluginManager, string> action) => _pluginManager.AddAction(actionName, _pluginType, action);

            void AddSimHubProperty(string propertyName, dynamic defaultValue) => _pluginManager.AddProperty(propertyName, _pluginType, defaultValue);
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

            SetSimHubProperty("Session_IsMultiClass", IsMultiClass);
        }

        private void OnDescriptionChanged()
        {
            Reset();

            SetSimHubProperty("Session_Description", Description);
        }

        private void ResetEstimatedLaps(PluginManager _, string __)
        {
            foreach (var driver in CarClasses.SelectMany(x => x.Teams).SelectMany(x => x.Drivers))
            {
                driver.BestLap = null;
            }
        }

        private void SetSimHubProperty(string propertyName, dynamic value) => _pluginManager.SetPropertyValue(propertyName, _pluginType, value);

        #region Interface: IDisposable
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}