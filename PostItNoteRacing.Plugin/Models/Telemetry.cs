namespace PostItNoteRacing.Plugin.Models
{
    /// <summary>
    /// Telemetry class, make sure it can be correctly serialized using JSON.net.
    /// </summary>
    internal class Telemetry
    {
        public bool EnableGapCalculations { get; set; } = true;

        public bool EnableInverseGapStrings { get; set; } = false;

        public bool EnableTelemetry { get; set; } = true;

        public int NLaps { get; set; } = 5;

        public bool UseLastNLapsToEstimateLapTime { get; set; } = false;
    }
}