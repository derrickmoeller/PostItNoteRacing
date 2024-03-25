using System;
using System.Collections.Generic;

namespace PostItNoteRacing.Plugin
{
    internal class Team
    {
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

        public bool? IsConnected { get; set; }

        public bool? IsInPit { get; set; }

        public bool? IsPlayer { get; set; }

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
