namespace PostItNoteRacing.Plugin.Models
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net.
    /// </summary>
    internal class Settings
    {
        public bool EnableEstimatedLaps { get; set; } = false;

        public bool EnableRealGaps { get; set; } = false;
    }
}
