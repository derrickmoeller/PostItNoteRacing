using System;
using System.Linq;

namespace PostItNoteRacing.Plugin
{
    internal class Driver
    {
        private static readonly char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

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

        public CarClass CarClass { get; set; }

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

        public double? IRating { get; set; }

        public double? IRatingChange { get; set; }

        public string IRatingString => $"{(IRating ?? 0D) / 1000:0.0k}";

        public string IRatingLicenseCombinedString => $"{License.ShortString} {IRatingString}";

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

        public License License { get; set; }

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

        public string ShortName
        {
            get
            {
                if (Name != null)
                {
                    return $"{Name.Split(' ')[0].Substring(0, 1)}. {String.Join(" ", Name.Split(' ').Skip(1)).TrimEnd(digits)}";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
