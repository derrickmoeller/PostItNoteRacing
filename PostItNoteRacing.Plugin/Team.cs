using System;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Plugin
{
    internal class Team
    {
        private int _lapsCompleted = 0;

        public string BestLapColor
        {
            get
            {
                if (DeltaToBest == 0)
                {
                    return Colors.Purple;
                }
                else if (IsPlayer == true)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public TimeSpan BestLapTime { get; set; }

        public string CarNumber { get; set; }

        public double? CurrentLapHighPrecision { get; set; }

        public double? DeltaToBest { get; set; }

        public double? DeltaToPlayerBest { get; set; }

        public double? DeltaToPlayerLast { get; set; }

        public List<Driver> Drivers { get; set; }

        public double? GapToLeader { get; set; }

        public string GapToLeaderString { get; set; }

        public double? GapToPlayer { get; set; }

        public string GapToPlayerString { get; set; }

        public double? Interval { get; set; }

        public string IntervalString { get; set; }

        public int IRating => (int)Math.Round(Drivers.Where(x => x.IRating > 0).Sum(x => x.IRating.Value * x.LapsCompleted) / Drivers.Where(x => x.IRating > 0).Sum(x => x.LapsCompleted));

        public bool? IsConnected { get; set; }

        public bool? IsInPit { get; set; }

        public bool? IsPlayer { get; set; }

        public int LapsCompleted
        {
            get { return _lapsCompleted; }
            set
            {
                if (_lapsCompleted != value)
                {
                    _lapsCompleted = value;
                }

                if (Drivers.Sum(x => x.LapsCompleted) != LapsCompleted)
                {
                    var driver = Drivers.SingleOrDefault(x => x.IsActive == true);
                    if (driver != null)
                    {
                        driver.LapsCompleted = LapsCompleted - Drivers.Where(x => x.IsActive == false).Sum(x => x.LapsCompleted);
                    }
                }
            }
        }

        public string LastLapColor
        {
            get
            {
                if (LastLapTime.TotalSeconds > 0 && LastLapTime == BestLapTime)
                {
                    return Colors.Green;
                }
                else if (IsPlayer == true)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public TimeSpan LastLapTime { get; set; }

        public int LivePosition { get; set; } = -1;

        public int LivePositionInClass { get; set; } = -1;

        public string Name { get; set; }

        public double? RelativeGapToPlayer { get; set; }

        public string RelativeGapToPlayerColor
        {
            get
            {
                if (GapToPlayer < 0 && (GapToPlayerString.EndsWith("L") || RelativeGapToPlayer >= 0))
                {
                    return Colors.Orange;
                }
                else if (GapToPlayer > 0 && (GapToPlayerString.EndsWith("L") || RelativeGapToPlayer <= 0))
                {
                    return Colors.Blue;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public string RelativeGapToPlayerString => $"{RelativeGapToPlayer:-0.0;+0.0}";
    }
}
