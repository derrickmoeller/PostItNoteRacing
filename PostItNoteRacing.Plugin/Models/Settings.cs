namespace PostItNoteRacing.Plugin.Models
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net.
    /// </summary>
    internal class Settings
    {
        public bool EnableEstimatedLapTimes { get; set; } = false;

        public bool EnableGapCalculations { get; set; } = false;

        public bool EnableTelemetry { get; set; } = true;

        public bool EnableUtility { get; set; } = false;

        public int IntegerAMax { get; set; } = 10;

        public int IntegerBMax { get; set; } = 10;

        public int IntegerCMax { get; set; } = 10;

        public int IntegerDMax { get; set; } = 10;
    }
}