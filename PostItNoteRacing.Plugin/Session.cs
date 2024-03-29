using GameReaderCommon;
using IRacingReader;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Plugin
{
    internal class Session
    {
        private readonly List<CarClass> _carClasses = new List<CarClass>();
        private readonly PluginManager _pluginManager;
        private readonly Type _pluginType;
        private readonly double _weight = 1600 / Math.Log(2);

        private string _description;

        private string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnDescriptionChanged();
                }
            }
        }

        public StatusDataBase StatusDatabase { get; set; }

        public Session(PluginManager pluginManager, Type pluginType)
        {
            _pluginManager = pluginManager;
            _pluginType = pluginType;

            CreateSimHubProperties();
        }

        public void Refresh()
        {
            Description = StatusDatabase.SessionTypeName;

            foreach (var (opponent, i) in StatusDatabase.Opponents.Select((opponent, i) => (opponent, i)))
            {
                var carClass = _carClasses.SingleOrDefault(x => x.Color == opponent.CarClassColor);
                if (carClass == null)
                {
                    carClass = new CarClass
                    {
                        Color = opponent.CarClassColor,
                        Name = opponent.CarClass,
                        Teams = new List<Team>(),
                        TextColor = opponent.CarClassTextColor
                    };

                    _carClasses.Add(carClass);
                }

                var team = carClass.Teams.SingleOrDefault(x => x.CarNumber == opponent.CarNumber);
                if (team == null)
                {
                    team = new Team
                    {
                        BestLapTime = opponent.BestLapTime,
                        CarNumber = opponent.CarNumber,
                        CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                        LapsCompleted = (int)(opponent.CurrentLapHighPrecision ?? 0D),
                        LastLapTime = opponent.LastLapTime,
                        Name = opponent.TeamName,
                        RelativeGapToPlayer = opponent.RelativeGapToPlayer
                    };

                    carClass.Teams.Add(team);
                }
                else if (opponent.IsConnected == true)
                {
                    team.BestLapTime = opponent.BestLapTime;
                    team.CurrentLapHighPrecision = opponent.CurrentLapHighPrecision;
                    team.LapsCompleted = (int)(opponent.CurrentLapHighPrecision ?? 0D);
                    team.LastLapTime = opponent.LastLapTime;
                    team.RelativeGapToPlayer = opponent.RelativeGapToPlayer;
                }
                else if (opponent.IsConnected == false)
                {
                    team.RelativeGapToPlayer = null;
                }

                team.Drivers.ForEach(x => x.IsActive = false);
                team.GapToLeader = opponent.GaptoClassLeader ?? 0;
                team.GapToPlayer = opponent.GaptoPlayer;
                team.IsConnected = opponent.IsConnected;
                team.IsInPit = opponent.IsCarInPit;
                team.IsPlayer = opponent.IsPlayer;
                team.LeaderboardPosition = i + 1;
                team.LivePosition = -1;
                team.LivePositionInClass = -1;

                var driver = team.Drivers.SingleOrDefault(x => x.Name == opponent.Name);
                if (driver == null)
                {
                    driver = new Driver
                    {
                        IRating = opponent.IRacing_IRating,
                        IsActive = true,
                        License = new License
                        {
                            String = opponent.LicenceString
                        },
                        Name = opponent.Name
                    };

                    team.Drivers.Add(driver);
                }
                else
                {
                    driver.IsActive = true;
                }
            }

            foreach (var carClass in _carClasses)
            {
                foreach (var team in carClass.Teams)
                {
                    if (Description == "Race")
                    {
                        team.LivePosition = _carClasses.SelectMany(x => x.Teams).Count(x => x.CurrentLapHighPrecision > team.CurrentLapHighPrecision) + 1;
                        team.LivePositionInClass = carClass.Teams.Count(x => x.CurrentLapHighPrecision > team.CurrentLapHighPrecision) + 1;

                        if (team.IRating > 0)
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
                    }
                    else
                    {
                        if (team.BestLapTime.TotalSeconds > 0)
                        {
                            team.LivePosition = _carClasses.SelectMany(x => x.Teams).Count(x => x.BestLapTime < team.BestLapTime && x.BestLapTime.TotalSeconds > 0) + 1;
                            team.LivePositionInClass = carClass.Teams.Count(x => x.BestLapTime < team.BestLapTime && x.BestLapTime.TotalSeconds > 0) + 1;
                        }
                        else
                        {
                            team.LivePosition = _carClasses.SelectMany(x => x.Teams).Count(x => x.BestLapTime.TotalSeconds > 0 || x.LivePosition != -1) + 1;
                            team.LivePositionInClass = carClass.Teams.Count(x => x.BestLapTime.TotalSeconds > 0 || x.LivePositionInClass != -1) + 1;
                        }
                    }

                    team.DeltaToBest = team.BestLapTime.TotalSeconds > 0 ? (team.BestLapTime - carClass.Teams.Where(x => x.BestLapTime.TotalSeconds > 0).Min(x => x.BestLapTime)).TotalSeconds : default(double?);
                }
            }

            var player = _carClasses.SelectMany(x => x.Teams).SingleOrDefault(x => x.IsPlayer == true);

            foreach (var carClass in _carClasses)
            {
                var leader = carClass.Teams.SingleOrDefault(x => x.LivePositionInClass == 1);
                        
                foreach (var team in carClass.Teams.OrderBy(x => x.LivePositionInClass))
                {
                    if (leader != null)
                    {
                        team.GapToLeaderString = GetGapAsString(team, leader, team.GapToLeader);
                    }

                    if (player != null)
                    {
                        team.DeltaToPlayerBest = (team.BestLapTime - player.BestLapTime).TotalSeconds;
                        team.DeltaToPlayerLast = (team.LastLapTime - player.LastLapTime).TotalSeconds;
                        team.GapToPlayerString = GetGapAsString(team, player, team.GapToPlayer);
                    }

                    if (team.LivePositionInClass == 1)
                    {
                        team.Interval = 0d;
                        team.IntervalString = $"L{(int)(team.CurrentLapHighPrecision + 1)}";
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

                    SetSimHubProperty($"Class_{carClass.Index:D2}_{team.LivePositionInClass:D2}_LeaderboardPosition", team.LeaderboardPosition);

                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapColor", team.BestLapColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_BestLapTime", team.BestLapTime);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_CarNumber", team.CarNumber);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassColor", carClass.Color);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassIndex", carClass.Index);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassString", carClass.Name);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ClassTextColor", carClass.TextColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_CurrentLapHighPrecision", team.CurrentLapHighPrecision);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToBest", team.DeltaToBest);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerBest", team.DeltaToPlayerBest);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_DeltaToPlayerLast", team.DeltaToPlayerLast);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeader", team.GapToLeader);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToLeaderString", team.GapToLeaderString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayer", team.GapToPlayer);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_GapToPlayerString", team.GapToPlayerString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_Interval", team.Interval);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IntervalString", team.IntervalString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRating", team.Drivers.Single(x => x.IsActive).IRating);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingChange", team.Drivers.Single(x => x.IsActive).IRatingChange);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingString", team.Drivers.Single(x => x.IsActive).IRatingString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IRatingLicenseCombinedString", team.Drivers.Single(x => x.IsActive).IRatingLicenseCombinedString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IsConnected", team.IsConnected);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IsInPit", team.IsInPit);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_IsPlayer", team.IsPlayer);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LapsCompleted", team.Drivers.Single(x => x.IsActive).LapsCompleted);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LastFiveLapsAverage", team.LastFiveLapsAverage);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapColor", team.LastLapColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LastLapTime", team.LastLapTime);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseColor", team.Drivers.Single(x => x.IsActive).License.Color);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseShortString", team.Drivers.Single(x => x.IsActive).License.ShortString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseString", team.Drivers.Single(x => x.IsActive).License.String);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LicenseTextColor", team.Drivers.Single(x => x.IsActive).License.TextColor);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePosition", team.LivePosition);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_LivePositionInClass", team.LivePositionInClass);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_Name", team.Drivers.Single(x => x.IsActive).Name);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayer", team.RelativeGapToPlayer);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerColor", Description == "Race" ? team.RelativeGapToPlayerColor : Colors.White);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_RelativeGapToPlayerString", team.RelativeGapToPlayerString);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_ShortName", team.Drivers.Single(x => x.IsActive).ShortName);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamIRating", team.IRating);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamLapsCompleted", team.LapsCompleted);
                    SetSimHubProperty($"Drivers_{team.LeaderboardPosition:D2}_TeamName", team.Name);
                    SetSimHubProperty($"Drivers_Live_{team.LivePosition:D2}_LeaderboardPosition", team.LeaderboardPosition);
                }

                int strengthOfField = GetStrengthOfField(carClass.Teams.Where(x => x.IRating > 0).Select(x => x.IRating.Value));

                SetSimHubProperty($"Class_{carClass.Index:D2}_SoF", strengthOfField);
                SetSimHubProperty($"Class_{carClass.Index:D2}_SoFString", $"{strengthOfField / 1000D:0.0k}");
            }

            var iRacingData = StatusDatabase.GetRawDataObject() as DataSampleEx;
            if (iRacingData != null)
            {
                iRacingData.Telemetry.TryGetValue("PlayerCarTeamIncidentCount", out object rawIncidents);

                SetSimHubProperty("Player_Incidents", Convert.ToInt32(rawIncidents));
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

            double GetIRatingChange(int teamIRating, int position, IEnumerable<int> iRatings)
            {
                double factor = (iRatings.Count() / 2D - position) / 100D;
                double sum = -0.5;

                foreach (var iRating in iRatings)
                {
                    sum += (1 - Math.Exp(-teamIRating / _weight)) * Math.Exp(-iRating / _weight) / ((1 - Math.Exp(-iRating / _weight)) * Math.Exp(-teamIRating / _weight) + (1 - Math.Exp(-teamIRating / _weight)) * Math.Exp(-iRating / _weight));
                }

                return Math.Round((iRatings.Count() - position - sum - factor) * 200 / iRatings.Count());
            }

            int GetStrengthOfField(IEnumerable<int> iRatings)
            {
                double sum = 0;

                foreach (var iRating in  iRatings)
                {
                    sum += Math.Pow(2, -iRating / 1600D);
                }

                return (int)Math.Round(_weight * Math.Log(iRatings.Count() / sum));
            }
        }

        public void Reset()
        {
            _carClasses.Clear();

            for (int i = 1; i <= 63; i++)
            {
                SetSimHubProperty($"Drivers_{i:D2}_BestLapColor", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_CarNumber", -1);
                SetSimHubProperty($"Drivers_{i:D2}_ClassColor", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_ClassIndex", -1);
                SetSimHubProperty($"Drivers_{i:D2}_ClassString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_ClassTextColor", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                SetSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                SetSimHubProperty($"Drivers_{i:D2}_GapToLeader", 0);
                SetSimHubProperty($"Drivers_{i:D2}_GapToLeaderString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                SetSimHubProperty($"Drivers_{i:D2}_GapToPlayerString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_Interval", 0);
                SetSimHubProperty($"Drivers_{i:D2}_IntervalString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_IRating", 0);
                SetSimHubProperty($"Drivers_{i:D2}_IRatingChange", 0);
                SetSimHubProperty($"Drivers_{i:D2}_IRatingString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_IsConnected", false);
                SetSimHubProperty($"Drivers_{i:D2}_IsInPit", false);
                SetSimHubProperty($"Drivers_{i:D2}_IsPlayer", false);
                SetSimHubProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                SetSimHubProperty($"Drivers_{i:D2}_LastFiveLapsAverage", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_LastLapColor", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseColor", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseShortString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LicenseTextColor", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_LivePosition", -1);
                SetSimHubProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                SetSimHubProperty($"Drivers_{i:D2}_Name", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                SetSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_ShortName", String.Empty);
                SetSimHubProperty($"Drivers_{i:D2}_TeamIRating", 0);
                SetSimHubProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                SetSimHubProperty($"Drivers_{i:D2}_TeamName", String.Empty);
                SetSimHubProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                SetSimHubProperty($"Class_{i:D2}_SoF", 0);
                SetSimHubProperty($"Class_{i:D2}_SoFString", String.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    SetSimHubProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            SetSimHubProperty("Player_Incidents", 0);
        }

        private void CreateSimHubProperties()
        {
            for (int i = 1; i <= 63; i++)
            {
                AddSimHubProperty($"Drivers_{i:D2}_BestLapColor", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_CarNumber", -1);
                AddSimHubProperty($"Drivers_{i:D2}_ClassColor", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_ClassIndex", -1);
                AddSimHubProperty($"Drivers_{i:D2}_ClassString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_ClassTextColor", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                AddSimHubProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                AddSimHubProperty($"Drivers_{i:D2}_GapToLeader", 0);
                AddSimHubProperty($"Drivers_{i:D2}_GapToLeaderString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                AddSimHubProperty($"Drivers_{i:D2}_GapToPlayerString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_Interval", 0);
                AddSimHubProperty($"Drivers_{i:D2}_IntervalString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_IRating", 0);
                AddSimHubProperty($"Drivers_{i:D2}_IRatingChange", 0);
                AddSimHubProperty($"Drivers_{i:D2}_IRatingString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_IsConnected", false);
                AddSimHubProperty($"Drivers_{i:D2}_IsInPit", false);
                AddSimHubProperty($"Drivers_{i:D2}_IsPlayer", false);
                AddSimHubProperty($"Drivers_{i:D2}_LapsCompleted", 0);
                AddSimHubProperty($"Drivers_{i:D2}_LastFiveLapsAverage", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_LastLapColor", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseColor", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseShortString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LicenseTextColor", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_LivePosition", -1);
                AddSimHubProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                AddSimHubProperty($"Drivers_{i:D2}_Name", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                AddSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_ShortName", String.Empty);
                AddSimHubProperty($"Drivers_{i:D2}_TeamIRating", 0);
                AddSimHubProperty($"Drivers_{i:D2}_TeamLapsCompleted", 0);
                AddSimHubProperty($"Drivers_{i:D2}_TeamName", String.Empty);
                AddSimHubProperty($"Drivers_Live_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                AddSimHubProperty($"Class_{i:D2}_SoF", 0);
                AddSimHubProperty($"Class_{i:D2}_SoFString", String.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    AddSimHubProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            AddSimHubProperty("Player_Incidents", 0);
            AddSimHubProperty("Version", "1.0.2.0");

            void AddSimHubProperty(string propertyName, dynamic defaultValue) => _pluginManager.AddProperty(propertyName, _pluginType, defaultValue);
        }

        private void OnDescriptionChanged()
        {
            Reset();
        }

        private void SetSimHubProperty(string propertyName, dynamic value) => _pluginManager.SetPropertyValue(propertyName, _pluginType, value);
    }
}