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
        private readonly List<Driver> _drivers = new List<Driver>();
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
            _drivers.Clear();

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
                        var driver = _drivers.SingleOrDefault(x => x.CarNumber == opponent.CarNumber);
                        if (driver == null)
                        {
                            driver = new Driver
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
                                IRating = opponent.IRacing_IRating,
                                LastLapTime = opponent.LastLapTime,
                                License = new License
                                {
                                    String = opponent.LicenceString
                                },
                                Name = opponent.Name,
                                RelativeGapToPlayer = opponent.RelativeGapToPlayer,
                            };

                            _drivers.Add(driver);
                        }
                        else if (opponent.IsConnected == true)
                        {
                            driver.BestLapTime = opponent.BestLapTime;
                            driver.CurrentLapHighPrecision = opponent.CurrentLapHighPrecision;
                            driver.IRating = opponent.IRacing_IRating;
                            driver.LastLapTime = opponent.LastLapTime;
                            driver.License.String = opponent.LicenceString;
                            driver.Name = opponent.Name;
                            driver.RelativeGapToPlayer = opponent.RelativeGapToPlayer;
                        }
                        else if (opponent.IsConnected == false)
                        {
                            driver.RelativeGapToPlayer = null;
                        }

                        driver.GapToLeader = opponent.GaptoClassLeader ?? 0;
                        driver.GapToPlayer = opponent.GaptoPlayer;
                        driver.IsConnected = opponent.IsConnected;
                        driver.IsInPit = opponent.IsCarInPit;
                        driver.IsPlayer = opponent.IsPlayer;
                        driver.LivePosition = -1;
                        driver.LivePositionInClass = -1;
                    }

                    foreach (var driver in _drivers)
                    {
                        if (SessionType == "Race")
                        {
                            driver.LivePosition = _drivers.Count(x => x.CurrentLapHighPrecision > driver.CurrentLapHighPrecision) + 1;
                            driver.LivePositionInClass = _drivers.Count(x => x.CarClass.Index == driver.CarClass.Index && x.CurrentLapHighPrecision > driver.CurrentLapHighPrecision) + 1;

                            if (driver.IRating > 0)
                            {
                                driver.IRatingChange = GetIRatingChange(driver.IRating.Value, driver.LivePositionInClass, _drivers.Where(x => x.CarClass.Index == driver.CarClass.Index && x.IRating > 0).Select(x => x.IRating.Value));
                            }
                        }
                        else
                        {
                            if (driver.BestLapTime.TotalSeconds > 0)
                            {
                                driver.LivePosition = _drivers.Count(x => x.BestLapTime < driver.BestLapTime && x.BestLapTime.TotalSeconds > 0) + 1;
                                driver.LivePositionInClass = _drivers.Count(x => x.CarClass.Index == driver.CarClass.Index && x.BestLapTime < driver.BestLapTime && x.BestLapTime.TotalSeconds > 0) + 1;
                            }
                            else
                            {
                                driver.LivePosition = _drivers.Count(x => x.BestLapTime.TotalSeconds > 0 || x.LivePosition != -1) + 1;
                                driver.LivePositionInClass = _drivers.Count(x => x.CarClass.Index == driver.CarClass.Index && (x.BestLapTime.TotalSeconds > 0 || x.LivePositionInClass != -1)) + 1;
                            }
                        }

                        driver.DeltaToBest = driver.BestLapTime.TotalSeconds > 0 ? (driver.BestLapTime - _drivers.Where(x => x.CarClass.Index == driver.CarClass.Index && x.BestLapTime.TotalSeconds > 0).Min(x => x.BestLapTime)).TotalSeconds : default(double?);
                    }

                    var player = _drivers.SingleOrDefault(x => x.IsPlayer == true);

                    foreach (var carClass in _drivers.GroupBy(x => x.CarClass.Index))
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
                                var driverAhead = carClass.SingleOrDefault(x => x.LivePositionInClass == driver.LivePositionInClass - 1);
                                if (driverAhead != null)
                                {
                                    driver.Interval = driver.GapToLeader - driverAhead.GapToLeader;
                                    driver.IntervalString = GetIntervalAsString(driver, driverAhead, driver.Interval);
                                }
                            }
                        }

                        int strengthOfField = GetStrengthOfField(carClass.Where(x => x.IRating > 0).Select(x => x.IRating.Value));

                        SetProperty($"Class_{carClass.Key:D2}_Best_LivePosition", carClass.Where(x => x.BestLapTime.TotalSeconds > 0).OrderBy(x => x.BestLapTime).FirstOrDefault()?.LivePosition ?? -1);
                        SetProperty($"Class_{carClass.Key:D2}_SoF", strengthOfField);
                        SetProperty($"Class_{carClass.Key:D2}_SoFString", $"{strengthOfField / 1000D:0.0k}");
                    }

                    foreach (var (driver, i) in _drivers.OrderBy(x => x.LivePosition).Select((driver, i) => (driver, i)))
                    {
                        SetProperty($"Class_{driver.CarClass.Index:D2}_{driver.LivePositionInClass:D2}_LivePosition", driver.LivePosition);

                        SetProperty($"Drivers_{i + 1:D2}_BestLapColor", driver.BestLapColor);
                        SetProperty($"Drivers_{i + 1:D2}_BestLapTime", driver.BestLapTime);
                        SetProperty($"Drivers_{i + 1:D2}_CarNumber", driver.CarNumber);
                        SetProperty($"Drivers_{i + 1:D2}_ClassColor", driver.CarClass.Color);
                        SetProperty($"Drivers_{i + 1:D2}_ClassIndex", driver.CarClass.Index);
                        SetProperty($"Drivers_{i + 1:D2}_ClassString", driver.CarClass.Name);
                        SetProperty($"Drivers_{i + 1:D2}_ClassTextColor", driver.CarClass.TextColor);
                        SetProperty($"Drivers_{i + 1:D2}_CurrentLapHighPrecision", driver.CurrentLapHighPrecision);
                        SetProperty($"Drivers_{i + 1:D2}_DeltaToBest", driver.DeltaToBest);
                        SetProperty($"Drivers_{i + 1:D2}_DeltaToPlayerBest", driver.DeltaToPlayerBest);
                        SetProperty($"Drivers_{i + 1:D2}_DeltaToPlayerLast", driver.DeltaToPlayerLast);
                        SetProperty($"Drivers_{i + 1:D2}_GapToLeader", driver.GapToLeader);
                        SetProperty($"Drivers_{i + 1:D2}_GapToLeaderString", driver.GapToLeaderString);
                        SetProperty($"Drivers_{i + 1:D2}_GapToPlayer", driver.GapToPlayer);
                        SetProperty($"Drivers_{i + 1:D2}_GapToPlayerString", driver.GapToPlayerString);
                        SetProperty($"Drivers_{i + 1:D2}_Interval", driver.Interval);
                        SetProperty($"Drivers_{i + 1:D2}_IntervalString", driver.IntervalString);
                        SetProperty($"Drivers_{i + 1:D2}_IRating", driver.IRating);
                        SetProperty($"Drivers_{i + 1:D2}_IRatingChange", driver.IRatingChange);
                        SetProperty($"Drivers_{i + 1:D2}_IRatingString", driver.IRatingString);
                        SetProperty($"Drivers_{i + 1:D2}_IRatingLicenseCombinedString", driver.IRatingLicenseCombinedString);
                        SetProperty($"Drivers_{i + 1:D2}_IsConnected", driver.IsConnected);
                        SetProperty($"Drivers_{i + 1:D2}_IsInPit", driver.IsInPit);
                        SetProperty($"Drivers_{i + 1:D2}_IsPlayer", driver.IsPlayer);
                        SetProperty($"Drivers_{i + 1:D2}_LastLapColor", driver.LastLapColor);
                        SetProperty($"Drivers_{i + 1:D2}_LastLapTime", driver.LastLapTime);
                        SetProperty($"Drivers_{i + 1:D2}_LicenseColor", driver.License.Color);
                        SetProperty($"Drivers_{i + 1:D2}_LicenseShortString", driver.License.ShortString);
                        SetProperty($"Drivers_{i + 1:D2}_LicenseString", driver.License.String);
                        SetProperty($"Drivers_{i + 1:D2}_LicenseTextColor", driver.License.TextColor);
                        SetProperty($"Drivers_{i + 1:D2}_LivePosition", driver.LivePosition);
                        SetProperty($"Drivers_{i + 1:D2}_LivePositionInClass", driver.LivePositionInClass);
                        SetProperty($"Drivers_{i + 1:D2}_Name", driver.Name);
                        SetProperty($"Drivers_{i + 1:D2}_RelativeGapToPlayer", driver.RelativeGapToPlayer);
                        SetProperty($"Drivers_{i + 1:D2}_RelativeGapToPlayerColor", driver.RelativeGapToPlayerColor);
                        SetProperty($"Drivers_{i + 1:D2}_RelativeGapToPlayerString", driver.RelativeGapToPlayerString);
                        SetProperty($"Drivers_{i + 1:D2}_ShortName", driver.ShortName);
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var driverAhead = _drivers.Where(x => x.RelativeGapToPlayer < 0 && x.IsInPit == false).OrderByDescending(x => x.RelativeGapToPlayer).ElementAtOrDefault(i);
                        if (driverAhead != null)
                        {
                            SetProperty($"Ahead_{i + 1:D2}_LivePosition", driverAhead.LivePosition);
                        }
                        else
                        {
                            SetProperty($"Ahead_{i + 1:D2}_LivePosition", -1);
                        }

                        var driverBehind = _drivers.Where(x => x.RelativeGapToPlayer >= 0 && x.IsInPit == false).OrderBy(x => x.RelativeGapToPlayer).ElementAtOrDefault(i);
                        if (driverBehind != null)
                        {
                            SetProperty($"Behind_{i + 1:D2}_LivePosition", driverBehind.LivePosition);
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
            AddProperty("Version", "1.0.1.0");
        }
        #endregion
    }
}