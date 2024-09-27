namespace PostItNoteRacing.Plugin.Models
{
    /// <summary>
    /// Telemetry class, make sure it can be correctly serialized using JSON.net.
    /// </summary>
    internal class Telemetry
    {
        public bool EnableGapCalculations { get; set; } = true;

        public bool EnableInverseGapStrings { get; set; } = false;

        public int NLaps { get; set; } = 5;

        public int XLaps { get; set; } = 2;

        public bool OverrideJavaScriptFunctions { get; set; } = false;

        public ReferenceLap ReferenceLap { get; set; } = ReferenceLap.PersonalBest;
    }
}