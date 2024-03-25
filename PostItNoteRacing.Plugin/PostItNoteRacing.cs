using GameReaderCommon;
using IRacingReader;
using SimHub;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Plugin
{
    [PluginAuthor("Derrick Moeller")]
    [PluginDescription("Properties for iRacing")]
    [PluginName("PostItNoteRacing")]
    public class PostItNoteRacing : IDataPlugin
    {
        private readonly List<CarClass> _carClasses = new List<CarClass>();
        private readonly double _weight = 1600 / Math.Log(2);

        private string _sessionType;

        private string SessionType
        {
            get { return _sessionType; }
            set
            {
                if (_sessionType != value)
                {
                    _sessionType = value;
                    OnSessionTypeChanged();
                }
            }
        }

        private void AddProperty(string propertyName, dynamic defaultValue) => PluginManager.AddProperty(propertyName, typeof(PostItNoteRacing), defaultValue);

        private void SetProperty(string propertyName, dynamic value) => PluginManager.SetPropertyValue(propertyName, typeof(PostItNoteRacing), value);

        private void OnSessionTypeChanged()
        {
            _carClasses.Clear();

            ResetSimHubProperties();
        }

        private void ResetSimHubProperties()
        {
            for (int i = 1; i <= 63; i++)
            {
                SetProperty($"Drivers_{i:D2}_BestLapColor", String.Empty);
                SetProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                SetProperty($"Drivers_{i:D2}_CarNumber", -1);
                SetProperty($"Drivers_{i:D2}_ClassColor", String.Empty);
                SetProperty($"Drivers_{i:D2}_ClassIndex", -1);
                SetProperty($"Drivers_{i:D2}_ClassString", String.Empty);
                SetProperty($"Drivers_{i:D2}_ClassTextColor", String.Empty);
                SetProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                SetProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                SetProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                SetProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                SetProperty($"Drivers_{i:D2}_GapToLeader", 0);
                SetProperty($"Drivers_{i:D2}_GapToLeaderString", String.Empty);
                SetProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                SetProperty($"Drivers_{i:D2}_GapToPlayerString", String.Empty);
                SetProperty($"Drivers_{i:D2}_Interval", 0);
                SetProperty($"Drivers_{i:D2}_IntervalString", String.Empty);
                SetProperty($"Drivers_{i:D2}_IRating", 0);
                SetProperty($"Drivers_{i:D2}_IRatingChange", 0);
                SetProperty($"Drivers_{i:D2}_IRatingString", String.Empty);
                SetProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", String.Empty);
                SetProperty($"Drivers_{i:D2}_IsConnected", false);
                SetProperty($"Drivers_{i:D2}_IsInPit", false);
                SetProperty($"Drivers_{i:D2}_IsPlayer", false);
                SetProperty($"Drivers_{i:D2}_LastLapColor", String.Empty);
                SetProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                SetProperty($"Drivers_{i:D2}_LicenseColor", String.Empty);
                SetProperty($"Drivers_{i:D2}_LicenseShortString", String.Empty);
                SetProperty($"Drivers_{i:D2}_LicenseString", String.Empty);
                SetProperty($"Drivers_{i:D2}_LicenseTextColor", String.Empty);
                SetProperty($"Drivers_{i:D2}_LivePosition", -1);
                SetProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                SetProperty($"Drivers_{i:D2}_Name", String.Empty);
                SetProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                SetProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", String.Empty);
                SetProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", String.Empty);
                SetProperty($"Drivers_{i:D2}_ShortName", String.Empty);
                SetProperty($"Drivers_{i:D2}_TeamName", String.Empty);
            }

            for (int i = 1; i <= 5; i++)
            {
                SetProperty($"Ahead_{i:D2}_LivePosition", -1);
                SetProperty($"Behind_{i:D2}_LivePosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                SetProperty($"Class_{i:D2}_Best_LivePosition", -1);
                SetProperty($"Class_{i:D2}_SoF", 0);
                SetProperty($"Class_{i:D2}_SoFString", String.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    SetProperty($"Class_{i:D2}_{j:D2}_LivePosition", -1);
                }
            }

            SetProperty("Player_Incidents", 0);
            SetProperty("Player_LivePosition", -1);
        }

        #region Interface: IDataPlugin
        public PluginManager PluginManager { get; set; }

        public void DataUpdate(PluginManager _, ref GameData data)
        {
            try
            {
                if (data.GameRunning && data.NewData != null)
                {
                    SessionType = data.NewData.SessionTypeName;
                    
                    foreach (var opponent in data.NewData.Opponents)
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

                        var team = carClass.Teams.SingleOrDefault(x => x.Name == opponent.TeamName);
                        if (team == null)
                        {
                            team = new Team
                            {
                                BestLapTime = opponent.BestLapTime,
                                CarNumber = opponent.CarNumber,
                                CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                                Drivers = new List<Driver>(),
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
                            if (SessionType == "Race")
                            {
                                team.LivePosition = _carClasses.SelectMany(x => x.Teams).Count(x => x.CurrentLapHighPrecision > team.CurrentLapHighPrecision) + 1;
                                team.LivePositionInClass = carClass.Teams.Count(x => x.CurrentLapHighPrecision > team.CurrentLapHighPrecision) + 1;

                                foreach (var driver in team.Drivers)
                                {
                                    if (driver.IRating > 0)
                                    {
                                        driver.IRatingChange = GetIRatingChange(driver.IRating.Value, team.LivePositionInClass, team.Drivers.Count, carClass.Teams.Select(x => x.Drivers.Average(y => y.IRating)).Where(x => x > 0).Select(x => x.Value));
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

                            SetProperty($"Class_{carClass.Index:D2}_{team.LivePositionInClass:D2}_LivePosition", team.LivePosition);

                            SetProperty($"Drivers_{team.LivePosition:D2}_BestLapColor", team.BestLapColor);
                            SetProperty($"Drivers_{team.LivePosition:D2}_BestLapTime", team.BestLapTime);
                            SetProperty($"Drivers_{team.LivePosition:D2}_CarNumber", team.CarNumber);
                            SetProperty($"Drivers_{team.LivePosition:D2}_ClassColor", carClass.Color);
                            SetProperty($"Drivers_{team.LivePosition:D2}_ClassIndex", carClass.Index);
                            SetProperty($"Drivers_{team.LivePosition:D2}_ClassString", carClass.Name);
                            SetProperty($"Drivers_{team.LivePosition:D2}_ClassTextColor", carClass.TextColor);
                            SetProperty($"Drivers_{team.LivePosition:D2}_CurrentLapHighPrecision", team.CurrentLapHighPrecision);
                            SetProperty($"Drivers_{team.LivePosition:D2}_DeltaToBest", team.DeltaToBest);
                            SetProperty($"Drivers_{team.LivePosition:D2}_DeltaToPlayerBest", team.DeltaToPlayerBest);
                            SetProperty($"Drivers_{team.LivePosition:D2}_DeltaToPlayerLast", team.DeltaToPlayerLast);
                            SetProperty($"Drivers_{team.LivePosition:D2}_GapToLeader", team.GapToLeader);
                            SetProperty($"Drivers_{team.LivePosition:D2}_GapToLeaderString", team.GapToLeaderString);
                            SetProperty($"Drivers_{team.LivePosition:D2}_GapToPlayer", team.GapToPlayer);
                            SetProperty($"Drivers_{team.LivePosition:D2}_GapToPlayerString", team.GapToPlayerString);
                            SetProperty($"Drivers_{team.LivePosition:D2}_Interval", team.Interval);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IntervalString", team.IntervalString);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IRating", team.Drivers.Single(x => x.IsActive).IRating);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IRatingChange", team.Drivers.Single(x => x.IsActive).IRatingChange);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IRatingString", team.Drivers.Single(x => x.IsActive).IRatingString);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IRatingLicenseCombinedString", team.Drivers.Single(x => x.IsActive).IRatingLicenseCombinedString);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IsConnected", team.IsConnected);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IsInPit", team.IsInPit);
                            SetProperty($"Drivers_{team.LivePosition:D2}_IsPlayer", team.IsPlayer);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LastLapColor", team.LastLapColor);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LastLapTime", team.LastLapTime);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LicenseColor", team.Drivers.Single(x => x.IsActive).License.Color);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LicenseShortString", team.Drivers.Single(x => x.IsActive).License.ShortString);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LicenseString", team.Drivers.Single(x => x.IsActive).License.String);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LicenseTextColor", team.Drivers.Single(x => x.IsActive).License.TextColor);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LivePosition", team.LivePosition);
                            SetProperty($"Drivers_{team.LivePosition:D2}_LivePositionInClass", team.LivePositionInClass);
                            SetProperty($"Drivers_{team.LivePosition:D2}_Name", team.Drivers.Single(x => x.IsActive).Name);
                            SetProperty($"Drivers_{team.LivePosition:D2}_RelativeGapToPlayer", team.RelativeGapToPlayer);
                            SetProperty($"Drivers_{team.LivePosition:D2}_RelativeGapToPlayerColor", team.RelativeGapToPlayerColor);
                            SetProperty($"Drivers_{team.LivePosition:D2}_RelativeGapToPlayerString", team.RelativeGapToPlayerString);
                            SetProperty($"Drivers_{team.LivePosition:D2}_ShortName", team.Drivers.Single(x => x.IsActive).ShortName);
                            SetProperty($"Drivers_{team.LivePosition:D2}_TeamName", team.Name);
                        }

                        int strengthOfField = GetStrengthOfField(carClass.Teams.Select(x => x.Drivers.Average(y => y.IRating)).Where(x => x > 0).Select(x => x.Value));

                        SetProperty($"Class_{carClass.Index:D2}_Best_LivePosition", carClass.Teams.Where(x => x.BestLapTime.TotalSeconds > 0).OrderBy(x => x.BestLapTime).FirstOrDefault()?.LivePosition ?? -1);
                        SetProperty($"Class_{carClass.Index:D2}_SoF", strengthOfField);
                        SetProperty($"Class_{carClass.Index:D2}_SoFString", $"{strengthOfField / 1000D:0.0k}");
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var teamAhead = _carClasses.SelectMany(x => x.Teams).Where(x => x.RelativeGapToPlayer < 0 && x.IsInPit == false).OrderByDescending(x => x.RelativeGapToPlayer).ElementAtOrDefault(i);
                        if (teamAhead != null)
                        {
                            SetProperty($"Ahead_{i + 1:D2}_LivePosition", teamAhead.LivePosition);
                        }
                        else
                        {
                            SetProperty($"Ahead_{i + 1:D2}_LivePosition", -1);
                        }

                        var teamBehind = _carClasses.SelectMany(x => x.Teams).Where(x => x.RelativeGapToPlayer >= 0 && x.IsInPit == false).OrderBy(x => x.RelativeGapToPlayer).ElementAtOrDefault(i);
                        if (teamBehind != null)
                        {
                            SetProperty($"Behind_{i + 1:D2}_LivePosition", teamBehind.LivePosition);
                        }
                        else
                        {
                            SetProperty($"Behind_{i + 1:D2}_LivePosition", -1);
                        }
                    }

                    if (data.GameName == "IRacing")
                    {
                        var iRacingData = data.NewData.GetRawDataObject() as DataSampleEx;
                        if (iRacingData != null)
                        {
                            iRacingData.Telemetry.TryGetValue("PlayerCarTeamIncidentCount", out object rawIncidents);

                            SetProperty("Player_Incidents", Convert.ToInt32(rawIncidents));
                        }
                    }

                    SetProperty("Player_LivePosition", player.LivePosition);
                }
                else
                {
                    SessionType = null;
                }
            }
            catch (Exception ex)
            {
                Logging.Current.Info($"Exception in plugin ({nameof(PostItNoteRacing)}) : {ex}");
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

            int GetIRatingChange(double driverIRating, int position, int numberOfTeammates, IEnumerable<double> iRatings)
            {
                double factor = (iRatings.Count() / 2 - position) / 100;
                double sum = -0.5;

                foreach (var iRating in iRatings)
                {
                    sum += (1 - Math.Exp(-driverIRating / _weight)) * Math.Exp(-iRating / _weight) / ((1 - Math.Exp(-iRating / _weight)) * Math.Exp(-driverIRating / _weight) + (1 - Math.Exp(-driverIRating / _weight)) * Math.Exp(-iRating / _weight));
                }

                return (int)(Math.Round((iRatings.Count() - position - sum - factor) * 200 / iRatings.Count()) / numberOfTeammates);
            }

            int GetStrengthOfField(IEnumerable<double> iRatings)
            {
                double sum = 0;

                foreach (var iRating in  iRatings)
                {
                    sum += Math.Pow(2, -iRating / 1600);
                }

                return (int)Math.Round(_weight * Math.Log(iRatings.Count() / sum));
            }
        }

        public void End(PluginManager _)
        {
            Logging.Current.Info($"Stopping plugin : {nameof(PostItNoteRacing)}");
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager _)
        {
            Logging.Current.Info($"Starting plugin : {nameof(PostItNoteRacing)}");

            for (int i = 1; i <= 63; i++)
            {
                AddProperty($"Drivers_{i:D2}_BestLapColor", String.Empty);
                AddProperty($"Drivers_{i:D2}_BestLapTime", TimeSpan.Zero);
                AddProperty($"Drivers_{i:D2}_CarNumber", -1);
                AddProperty($"Drivers_{i:D2}_ClassColor", String.Empty);
                AddProperty($"Drivers_{i:D2}_ClassIndex", -1);
                AddProperty($"Drivers_{i:D2}_ClassString", String.Empty);
                AddProperty($"Drivers_{i:D2}_ClassTextColor", String.Empty);
                AddProperty($"Drivers_{i:D2}_CurrentLapHighPrecision", 0);
                AddProperty($"Drivers_{i:D2}_DeltaToBest", 0);
                AddProperty($"Drivers_{i:D2}_DeltaToPlayerBest", 0);
                AddProperty($"Drivers_{i:D2}_DeltaToPlayerLast", 0);
                AddProperty($"Drivers_{i:D2}_GapToLeader", 0);
                AddProperty($"Drivers_{i:D2}_GapToLeaderString", String.Empty);
                AddProperty($"Drivers_{i:D2}_GapToPlayer", 0);
                AddProperty($"Drivers_{i:D2}_GapToPlayerString", String.Empty);
                AddProperty($"Drivers_{i:D2}_Interval", 0);
                AddProperty($"Drivers_{i:D2}_IntervalString", String.Empty);
                AddProperty($"Drivers_{i:D2}_IRating", 0);
                AddProperty($"Drivers_{i:D2}_IRatingChange", 0);
                AddProperty($"Drivers_{i:D2}_IRatingString", String.Empty);
                AddProperty($"Drivers_{i:D2}_IRatingLicenseCombinedString", String.Empty);
                AddProperty($"Drivers_{i:D2}_IsConnected", false);
                AddProperty($"Drivers_{i:D2}_IsInPit", false);
                AddProperty($"Drivers_{i:D2}_IsPlayer", false);
                AddProperty($"Drivers_{i:D2}_LastLapColor", String.Empty);
                AddProperty($"Drivers_{i:D2}_LastLapTime", TimeSpan.Zero);
                AddProperty($"Drivers_{i:D2}_LicenseColor", String.Empty);
                AddProperty($"Drivers_{i:D2}_LicenseShortString", String.Empty);
                AddProperty($"Drivers_{i:D2}_LicenseString", String.Empty);
                AddProperty($"Drivers_{i:D2}_LicenseTextColor", String.Empty);
                AddProperty($"Drivers_{i:D2}_LivePosition", -1);
                AddProperty($"Drivers_{i:D2}_LivePositionInClass", -1);
                AddProperty($"Drivers_{i:D2}_Name", String.Empty);
                AddProperty($"Drivers_{i:D2}_RelativeGapToPlayer", 0);
                AddProperty($"Drivers_{i:D2}_RelativeGapToPlayerColor", String.Empty);
                AddProperty($"Drivers_{i:D2}_RelativeGapToPlayerString", String.Empty);
                AddProperty($"Drivers_{i:D2}_ShortName", String.Empty);
                AddProperty($"Drivers_{i:D2}_TeamName", String.Empty);
            }

            for (int i = 1; i <= 5; i++)
            {
                AddProperty($"Ahead_{i:D2}_LivePosition", -1);
                AddProperty($"Behind_{i:D2}_LivePosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                AddProperty($"Class_{i:D2}_Best_LivePosition", -1);
                AddProperty($"Class_{i:D2}_SoF", 0);
                AddProperty($"Class_{i:D2}_SoFString", String.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    AddProperty($"Class_{i:D2}_{j:D2}_LivePosition", -1);
                }
            }

            AddProperty("Player_Incidents", 0);
            AddProperty("Player_LivePosition", -1);
            AddProperty("Version", "1.0.1.1");
        }
        #endregion
    }
}