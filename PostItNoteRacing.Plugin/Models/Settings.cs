using System.Collections.Generic;

namespace PostItNoteRacing.Plugin.Models
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net.
    /// </summary>
    internal class Settings
    {
        public int BooleanQuantity { get; set; } = 0;

        public bool EnableGapCalculations { get; set; } = false;

        public bool EnableTelemetry { get; set; } = true;

        public List<IntegerProperty> IntegerActions { get; set; } = new List<IntegerProperty>();

        public int NLaps { get; set; } = 5;
    }
}