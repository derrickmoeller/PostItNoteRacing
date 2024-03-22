using GameReaderCommon;
using SimHub;
using SimHub.Plugins;
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

        public void AddProperty(string propertyName, dynamic defaultValue) => PluginManager.AddProperty(propertyName, typeof(PostItNoteRacing), defaultValue);

        public void SetProperty(string propertyName, dynamic value) => PluginManager.SetPropertyValue(propertyName, typeof(PostItNoteRacing), value);

        #region Interface: IDataPlugin
        public PluginManager PluginManager { get; set; }

        public void DataUpdate(PluginManager _, ref GameData data)
        {
            if (data.GameName != "IRacing")
                return;

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
                        GapToLeader = opponent.GaptoLeader ?? 0,
                        GapToPlayer = opponent.GaptoPlayer,
                        IsConnected = opponent.IsConnected,
                        IsPlayer = opponent.IsPlayer,
                        LastLapTime = opponent.LastLapTime,
                        License = new License
                        {
                            String = opponent.LicenceString
                        },
                        Name = opponent.Name,
                        LeaderboardPosition = opponent.Position,
                        LeaderboardPositionInClass = opponent.PositionInClass,
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
                    }
                    else
                    {
                        driver.LeaderboardPosition = driver.LeaderboardPosition > 0 ? driver.LeaderboardPosition : drivers.Max(x => x.LeaderboardPosition) + 1;
                        driver.LivePosition = drivers.Count(x => x.BestLapTime < driver.BestLapTime && x.BestLapTime.TotalSeconds != 0) + 1;
                        driver.LivePositionInClass = drivers.Count(x => x.CarClass.Index == driver.CarClass.Index && x.BestLapTime < driver.BestLapTime && x.BestLapTime.TotalSeconds != 0) + 1;
                    }

                    driver.DeltaToBest = driver.BestLapTime.TotalSeconds != 0 ? (driver.BestLapTime - drivers.Where(x => x.CarClass.Index == driver.CarClass.Index && x.BestLapTime.TotalSeconds != 0).Min(x => x.BestLapTime)).TotalSeconds : default(double?);
                }

                var player = drivers.SingleOrDefault(x => x.IsPlayer);

                foreach (var carClass in drivers.GroupBy(x => x.CarClass.Index))
                {
                    var classLeader = carClass.SingleOrDefault(x => x.LivePositionInClass == 1);
                    double? gapToOverallLeader = classLeader?.GapToLeader;

                    foreach (var driver in carClass.OrderBy(x => x.LivePositionInClass))
                    {
                        driver.GapToLeader -= gapToOverallLeader;

                        if (classLeader != null)
                        {
                            driver.GapToLeaderString = GetGapAsString(driver, classLeader, driver.GapToLeader);
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
                    _drivers[i].IsConnected = driver.IsConnected;
                    _drivers[i].IsPlayer = driver.IsPlayer;
                    _drivers[i].LastLapTime = driver.LastLapTime;
                    _drivers[i].License.String = driver.License.String;
                    _drivers[i].LivePosition = driver.LivePosition;
                    _drivers[i].LivePositionInClass = driver.LivePositionInClass;
                    _drivers[i].Name = driver.Name;
                    _drivers[i].LeaderboardPosition = driver.LeaderboardPosition;
                    _drivers[i].LeaderboardPositionInClass = driver.LeaderboardPositionInClass;
                    _drivers[i].RelativeGapToPlayer = driver.RelativeGapToPlayer;

                    SetProperty($"Class_{driver.CarClass.Index:D2}_{driver.LivePositionInClass:D2}_LeaderboardPosition", driver.LeaderboardPosition);
                }

                SetProperty("Player_LeaderboardPosition", player.LeaderboardPosition);
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
        }

        public void End(PluginManager _)
        { }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager _)
        {
            Logging.Current.Info("Starting plugin : PostItNoteRacing");

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
                this.AttachDelegate($"Drivers_{i + 1:D2}_IsConnected", () => driver.IsConnected);
                this.AttachDelegate($"Drivers_{i + 1:D2}_IsPlayer", () => driver.IsPlayer);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LastLapColor", () => driver.LastLapColor);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LastLapTime", () => driver.LastLapTime);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LicenseColor", () => driver.License.Color);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LicenseString", () => driver.License.String);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LicenseTextColor", () => driver.License.TextColor);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LivePosition", () => driver.LivePosition);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LivePositionInClass", () => driver.LivePositionInClass);
                this.AttachDelegate($"Drivers_{i + 1:D2}_Name", () => driver.Name);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LeaderboardPosition", () => driver.LeaderboardPosition);
                this.AttachDelegate($"Drivers_{i + 1:D2}_LeaderboardPositionInClass", () => driver.LeaderboardPositionInClass);
                this.AttachDelegate($"Drivers_{i + 1:D2}_RelativeGapToPlayer", () => driver.RelativeGapToPlayer);
                this.AttachDelegate($"Drivers_{i + 1:D2}_RelativeGapToPlayerString", () => driver.RelativeGapToPlayerString);
                this.AttachDelegate($"Drivers_{i + 1:D2}_ShortName", () => driver.ShortName);
            }

            for (int i = 1; i <= CarClass.Colors.Count; i++)
            {
                for (int j = 1; j <= 63; j++)
                {
                    AddProperty($"Class_{i:D2}_{j:D2}_LeaderboardPosition", -1);
                }
            }

            AddProperty("Player_LeaderboardPosition", -1);
            AddProperty("Version", "1.00.0");
        }
        #endregion
    }
}