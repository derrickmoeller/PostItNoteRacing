namespace PostItNoteRacing.Plugin.Models
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net.
    /// </summary>
    internal class Settings
    {
        public bool EnableBooleans { get; set; } = false;

        public bool EnableEstimatedLapTimes { get; set; } = false;

        public bool EnableExtraProperties { get; set; } = true;

        public bool EnableGapCalculations { get; set; } = false;
    }
}
