using GameReaderCommon;
using IRacingReader;
using SimHub;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PostItNoteRacing.Plugin
{
    [PluginAuthor("Derrick Moeller")]
    [PluginDescription("Properties for iRacing")]
    [PluginName("PostItNoteRacing")]
    public class PostItNoteRacing : IDataPlugin
    {
        private readonly ReadOnlyCollection<Driver> _drivers = new ReadOnlyCollection<Driver>(Enumerable.Repeat(0, 63).Select(x => new Driver()).ToList());
        private readonly double _weight = 1600 / Math.Log(2);

        public void AddProperty(string propertyName, dynamic defaultValue) => PluginManager.AddProperty(propertyName, typeof(PostItNoteRacing), defaultValue);

        public void SetProperty(string propertyName, dynamic value) => PluginManager.SetPropertyValue(propertyName, typeof(PostItNoteRacing), value);

        private void InitializeSimHubProperties()
        {
            foreach (var driver in _drivers)
            {
                driver.BestLapTime = TimeSpan.Zero;
                driver.CarClass.Color = null;
                driver.CarClass.Name = null;
                driver.CarClass.TextColor = null;
                driver.CarNumber = null;
                driver.CurrentLapHighPrecision = null;
                driver.DeltaToBest = null;
                driver.DeltaToPlayerBest = null;
                driver.DeltaToPlayerLast = null;
                driver.GapToLeader = null;
                driver.GapToLeaderString = null;
                driver.GapToPlayer = null;
                driver.GapToPlayerString = null;
                driver.Interval = null;
                driver.IntervalString = null;
                driver.IRating = null;
                driver.IRatingChange = null;
                driver.IsConnected = null;
                driver.IsInPit = null;
                driver.IsPlayer = null;
                driver.LastLapTime = TimeSpan.Zero;
                driver.LeaderboardPosition = -1;
                driver.LeaderboardPositionInClass = -1;
                driver.License.String = null;
                driver.LivePosition = -1;
                driver.LivePositionInClass = -1;
                driver.Name = null;
                driver.RelativeGapToPlayer = null;
            }

            for (int i = 1; i <= 5; i++)
            {
                SetProperty($"Ahead_{i:D2}_LeaderboardPosition", -1);
                SetProperty($"Behind_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                SetProperty($"Class_{i:D2}_SoF", 0);
                SetProperty($"Class_{i:D2}_SoFString", String.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    SetProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            SetProperty("Player_Incidents", 0);
            SetProperty("Player_LeaderboardPosition", -1);
        }

        #region Interface: IDataPlugin
        public PluginManager PluginManager { get; set; }

        public void DataUpdate(PluginManager _, ref GameData data)
        {
            try
            {
                if (data.GameRunning && data.NewData != null)
                {
                    var drivers = new List<Driver>();

                    foreach (var opponent in data.NewData.Opponents)
                    {
                        var driver = new Driver
                        {
                            BestLapTime = opponent.BestLapTime,
                            CarClass = new CarClass
                            {
                                Color = opponent.CarClassColor,
                                Name = opponent.CarClass,
                                TextColor = opponent.CarClassTextColor
                            },
                            CarNumber = opponent.CarNumber,
                            CurrentLapHighPrecision = opponent.CurrentLapHighPrecision,
                            GapToLeader = opponent.GaptoClassLeader ?? 0,
                            GapToPlayer = opponent.GaptoPlayer,
                            IRating = opponent.IRacing_IRating,
                            IsConnected = opponent.IsConnected,
                            IsInPit = opponent.IsCarInPit,
                            IsPlayer = opponent.IsPlayer,
                            LastLapTime = opponent.LastLapTime,
                            LeaderboardPosition = opponent.Position,
                            LeaderboardPositionInClass = opponent.PositionInClass,
                            License = new License
                            {
                                String = opponent.LicenceString
                            },
                            LivePosition = -1,
                            LivePositionInClass = -1,
                            Name = opponent.Name,
                            RelativeGapToPlayer = opponent.RelativeGapToPlayer
                        };

                        drivers.Add(driver);
                    }

                    foreach (var driver in drivers)
                    {
                        if (data.NewData.SessionTypeName == "Race")
                        {
                            driver.LivePosition = drivers.Count(x => x.CurrentLapHighPrecision > driver.CurrentLapHighPrecision) + 1;
                            driver.LivePositionInClass = drivers.Count(x => x.CarClass.Index == driver.CarClass.Index && x.CurrentLapHighPrecision > driver.CurrentLapHighPrecision) + 1;

                            if (driver.IRating > 0)
                            {
                                driver.IRatingChange = GetIRatingChange(driver.IRating.Value, driver.LivePositionInClass, drivers.Where(x => x.CarClass.Index == driver.CarClass.Index && x.IRating > 0).Select(x => x.IRating.Value));
                            }
                        }
                        else
                        {
                            driver.LeaderboardPosition = driver.LeaderboardPosition > 0 ? driver.LeaderboardPosition : drivers.Max(x => x.LeaderboardPosition) + 1;
                            
                            if (driver.BestLapTime.TotalSeconds > 0)
                            {
                                driver.LivePosition = drivers.Count(x => x.BestLapTime < driver.BestLapTime && x.BestLapTime.TotalSeconds > 0) + 1;
                                driver.LivePositionInClass = drivers.Count(x => x.CarClass.Index == driver.CarClass.Index && x.BestLapTime < driver.BestLapTime && x.BestLapTime.TotalSeconds > 0) + 1;
                            }
                            else
                            {
                                driver.LivePosition = drivers.Count(x => x.BestLapTime.TotalSeconds > 0 || x.LivePosition != -1) + 1;
                                driver.LivePositionInClass = drivers.Count(x => x.CarClass.Index == driver.CarClass.Index && (x.BestLapTime.TotalSeconds > 0 || x.LivePositionInClass != -1)) + 1;
                            }
                        }

                        driver.DeltaToBest = driver.BestLapTime.TotalSeconds > 0 ? (driver.BestLapTime - drivers.Where(x => x.CarClass.Index == driver.CarClass.Index && x.BestLapTime.TotalSeconds > 0).Min(x => x.BestLapTime)).TotalSeconds : default(double?);
                    }

                    var player = drivers.SingleOrDefault(x => x.IsPlayer == true);

                    foreach (var carClass in drivers.GroupBy(x => x.CarClass.Index))
                    {
                        var leader = carClass.SingleOrDefault(x => x.LivePositionInClass == 1);
                        
                        foreach (var driver in carClass.OrderBy(x => x.LivePositionInClass))
                        {
                            if (leader != null)
                            {
                                driver.GapToLeaderString = GetGapAsString(driver, leader, driver.GapToLeader);
                            }

                            if (player != null)
                            {
                                driver.DeltaToPlayerBest = (driver.BestLapTime - player.BestLapTime).TotalSeconds;
                                driver.DeltaToPlayerLast = (driver.LastLapTime - player.LastLapTime).TotalSeconds;
                                driver.GapToPlayerString = GetGapAsString(driver, player, driver.GapToPlayer);
                            }

                            if (driver.LivePositionInClass == 1)
                            {
                                driver.Interval = 0d;
                                driver.IntervalString = $"L{(int)(driver.CurrentLapHighPrecision + 1)}";
                            }
                            else
                            {
                                var driverAhead = drivers.SingleOrDefault(x => x.CarClass.Index == driver.CarClass.Index && x.LivePositionInClass == driver.LivePositionInClass - 1);
                                if (driverAhead != null)
                                {
                                    driver.Interval = driver.GapToLeader - driverAhead.GapToLeader;
                                    driver.IntervalString = GetIntervalAsString(driver, driverAhead, driver.Interval);
                                }
                            }
                        }

                        int strengthOfField = GetStrengthOfField(carClass.Where(x => x.IRating > 0).Select(x => x.IRating.Value));

                        SetProperty($"Class_{carClass.Key:D2}_SoF", strengthOfField);
                        SetProperty($"Class_{carClass.Key:D2}_SoFString", $"{strengthOfField / 1000D:0.0k}");
                    }

                    foreach (var (driver, i) in drivers.OrderBy(x => x.LeaderboardPosition).Select((driver, i) => (driver, i)))
                    {
                        _drivers[i].BestLapTime = driver.BestLapTime;
                        _drivers[i].CarClass.Color = driver.CarClass.Color;
                        _drivers[i].CarClass.Name = driver.CarClass.Name;
                        _drivers[i].CarClass.TextColor = driver.CarClass.TextColor;
                        _drivers[i].CarNumber = driver.CarNumber;
                        _drivers[i].CurrentLapHighPrecision = driver.CurrentLapHighPrecision;
                        _drivers[i].DeltaToBest = driver.DeltaToBest;
                        _drivers[i].DeltaToPlayerBest = driver.DeltaToPlayerBest;
                        _drivers[i].DeltaToPlayerLast = driver.DeltaToPlayerLast;
                        _drivers[i].GapToLeader = driver.GapToLeader;
                        _drivers[i].GapToLeaderString = driver.GapToLeaderString;
                        _drivers[i].GapToPlayer = driver.GapToPlayer;
                        _drivers[i].GapToPlayerString = driver.GapToPlayerString;
                        _drivers[i].Interval = driver.Interval;
                        _drivers[i].IntervalString = driver.IntervalString;
                        _drivers[i].IRating = driver.IRating;
                        _drivers[i].IRatingChange = driver.IRatingChange;
                        _drivers[i].IsConnected = driver.IsConnected;
                        _drivers[i].IsInPit = driver.IsInPit;
                        _drivers[i].IsPlayer = driver.IsPlayer;
                        _drivers[i].LastLapTime = driver.LastLapTime;
                        _drivers[i].LeaderboardPosition = driver.LeaderboardPosition;
                        _drivers[i].LeaderboardPositionInClass = driver.LeaderboardPositionInClass;
                        _drivers[i].License.String = driver.License.String;
                        _drivers[i].LivePosition = driver.LivePosition;
                        _drivers[i].LivePositionInClass = driver.LivePositionInClass;
                        _drivers[i].Name = driver.Name;
                        _drivers[i].RelativeGapToPlayer = driver.RelativeGapToPlayer;

                        SetProperty($"Class_{driver.CarClass.Index:D2}_{driver.LivePositionInClass:D2}_LeaderboardPosition", driver.LeaderboardPosition);
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var driverAhead = drivers.Where(x => x.RelativeGapToPlayer < 0 && x.IsInPit == false).OrderByDescending(x => x.RelativeGapToPlayer).ElementAtOrDefault(i);
                        if (driverAhead != null)
                        {
                            SetProperty($"Ahead_{i + 1:D2}_LeaderboardPosition", driverAhead.LeaderboardPosition);
                        }
                        else
                        {
                            SetProperty($"Ahead_{i + 1:D2}_LeaderboardPosition", -1);
                        }

                        var driverBehind = drivers.Where(x => x.RelativeGapToPlayer >= 0 && x.IsInPit == false).OrderBy(x => x.RelativeGapToPlayer).ElementAtOrDefault(i);
                        if (driverBehind != null)
                        {
                            SetProperty($"Behind_{i + 1:D2}_LeaderboardPosition", driverBehind.LeaderboardPosition);
                        }
                        else
                        {
                            SetProperty($"Behind_{i + 1:D2}_LeaderboardPosition", -1);
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

                    SetProperty("Player_LeaderboardPosition", player.LeaderboardPosition);
                }
                else
                {
                    InitializeSimHubProperties();
                }
            }
            catch (Exception ex)
            {
                Logging.Current.Info($"Exception in plugin ({nameof(PostItNoteRacing)}) : {ex.Message}");
            }

            string GetGapAsString(Driver a, Driver b, double? gap)
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

            string GetIntervalAsString(Driver a, Driver b, double? interval)
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

            int GetIRatingChange(double driverIRating, int position, IEnumerable<double> iRatings)
            {
                double factor = (iRatings.Count() / 2 - position) / 100;
                double sum = -0.5;

                foreach (var iRating in iRatings)
                {
                    sum += (1 - Math.Exp(-driverIRating / _weight)) * Math.Exp(-iRating / _weight) / ((1 - Math.Exp(-iRating / _weight)) * Math.Exp(-driverIRating / _weight) + (1 - Math.Exp(-driverIRating / _weight)) * Math.Exp(-iRating / _weight));
                }

                return (int)Math.Round((iRatings.Count() - position - sum - factor) * 200 / iRatings.Count());
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

            foreach (var (driver, i) in _drivers.Select((driver, i) => (driver, i)))
            {
                this.AttachDelegate($"Drivers_{i + 1:D2}_BestLapColor", () => driver.BestLapColor);
                this.AttachDelegate($"Drivers_{i + 1:D2}_BestLapTime", () => driver.BestLapTime);
                this.AttachDelegate($"Drivers_{i + 1:D2}_CarNumber", () => driver.CarNumber);
                this.AttachDelegate($"Drivers_{i + 1:D2}_ClassColor", () => driver.CarClass.Color);
                this.AttachDelegate($"Drivers_{i + 1:D2}_ClassIndex", () => driver.CarClass.Index);
                this.AttachDelegate($"Drivers_{i + 1:D2}_ClassString", () => driver.CarClass.Name);
                this.AttachDelegate($"Drivers_{i + 1:D2}_ClassTextColor", () => driver.CarClass.TextColor);
                this.AttachDelegate($"Drivers_{i + 1:D2}_CurrentLapHighPrecision", () => driver.CurrentLapHighPrecision);
                this.AttachDelegate($"Drivers_{i + 1:D2}_DeltaToBest", () => driver.DeltaToBest);
                this.AttachDelegate($"Drivers_{i + 1:D2}_DeltaToPlayerBest", () => driver.DeltaToPlayerBest);
                this.AttachDelegate($"Drivers_{i + 1:D2}_DeltaToPlayerLast", () => driver.DeltaToPlayerLast);
                this.AttachDelegate($"Drivers_{i + 1:D2}_GapToLeader", () => driver.GapToLeader);
                this.AttachDelegate($"Drivers_{i + 1:D2}_GapToLeaderString", () => driver.GapToLeaderString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_GapToPlayer", () => driver.GapToPlayer);
                this.AttachDelegate($"Drivers_{i + 1:D2}_GapToPlayerString", () => driver.GapToPlayerString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_Interval", () => driver.Interval);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IntervalString", () => driver.IntervalString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IRating", () => driver.IRating);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IRatingChange", () => driver.IRatingChange);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IRatingString", () => driver.IRatingString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IRatingLicenseCombinedString", () => driver.IRatingLicenseCombinedString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IsConnected", () => driver.IsConnected);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IsInPit", () => driver.IsInPit);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IsPlayer", () => driver.IsPlayer);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LastLapColor", () => driver.LastLapColor);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LastLapTime", () => driver.LastLapTime);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LeaderboardPosition", () => driver.LeaderboardPosition);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LeaderboardPositionInClass", () => driver.LeaderboardPositionInClass);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LicenseColor", () => driver.License.Color);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LicenseShortString", () => driver.License.ShortString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LicenseString", () => driver.License.String);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LicenseTextColor", () => driver.License.TextColor);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LivePosition", () => driver.LivePosition);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LivePositionInClass", () => driver.LivePositionInClass);
                this.AttachDelegate($"Drivers_{i + 1:D2}_Name", () => driver.Name);
                this.AttachDelegate($"Drivers_{i + 1:D2}_RelativeGapToPlayer", () => driver.RelativeGapToPlayer);
                this.AttachDelegate($"Drivers_{i + 1:D2}_RelativeGapToPlayerColor", () => driver.RelativeGapToPlayerColor);
                this.AttachDelegate($"Drivers_{i + 1:D2}_RelativeGapToPlayerString", () => driver.RelativeGapToPlayerString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_ShortName", () => driver.ShortName);
            }

            for (int i = 1; i <= 5; i++)
            {
                AddProperty($"Ahead_{i:D2}_LeaderboardPosition", -1);
                AddProperty($"Behind_{i:D2}_LeaderboardPosition", -1);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                AddProperty($"Class_{i:D2}_SoF", 0);
                AddProperty($"Class_{i:D2}_SoFString", String.Empty);

                for (int j = 1; j <= 63; j++)
                {
                    AddProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            AddProperty("Player_Incidents", 0);
            AddProperty("Player_LeaderboardPosition", -1);
            AddProperty("Version", "1.0.0.2");
        }
        #endregion
    }
}