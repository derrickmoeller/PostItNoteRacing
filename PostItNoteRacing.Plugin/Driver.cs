using System;

namespace PostItNoteRacing.Plugin
{
    internal class Driver
    {
        public string BestLapColor
        {
            get
            {
                if (DeltaToBest == 0)
                {
                    return Colors.Purple;
                }
                else if (IsPlayer)
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

        public CarClass CarClass { get; set; } = new CarClass();

        public string CarNumber { get; set; }

        public double? CurrentLapHighPrecision { get; set; }

        public double? DeltaToBest { get; set; }

        public double? DeltaToPlayerBest { get; set; }

        public double? DeltaToPlayerLast { get; set; }

        public double? GapToLeader { get; set; }

        public string GapToLeaderString { get; set; }

        public double? GapToPlayer { get; set; }

        public string GapToPlayerString { get; set; }

        public double? Interval { get; set; }

        public string IntervalString { get; set; }

        public bool IsConnected { get; set; }

        public bool IsPlayer { get; set; }

        public string LastLapColor
        {
            get
            {
                if (LastLapTime == BestLapTime)
                {
                    return Colors.Green;
                }
                else if (IsPlayer)
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

        public License License { get; set; } = new License();

        public int LivePosition { get; set; } = -1;

        public int LivePositionInClass { get; set; } = -1;

        public string Name { get; set; }

        public int LeaderboardPosition { get; set; } = -1;

        public int LeaderboardPositionInClass { get; set; } = -1;

        public double? RelativeGapToPlayer { get; set; }

        public string RelativeGapToPlayerString => $"{RelativeGapToPlayer:-0.0;+0.0}";
    }
}
