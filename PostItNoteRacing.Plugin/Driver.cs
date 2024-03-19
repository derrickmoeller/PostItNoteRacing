namespace PostItNoteRacing.Plugin
{
    internal class Driver
    {
        public CarClass CarClass { get; set; } = new CarClass();

        public double? CurrentLapHighPrecision { get; set; }

        public double? GapToLeader { get; set; }

        public string GapToLeaderString { get; set; }

        public double? GapToPlayer { get; set; }

        public string GapToPlayerString { get; set; }

        public double? Interval { get; set; }

        public string IntervalString { get; set; }

        public bool IsConnected { get; set; }

        public bool IsPlayer { get; set; }

        public License License { get; set; } = new License();

        public int? LivePosition { get; set; }

        public int? LivePositionInClass { get; set; }

        public string Name { get; set; }

        public int? Position { get; set; }

        public int? PositionInClass { get; set; }

        public double? RelativeGapToPlayer { get; set; }

        public string RelativeGapToPlayerString => $"{RelativeGapToPlayer:-0.0;+0.0}";
    }
}
