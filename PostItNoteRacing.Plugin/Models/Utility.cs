using System.Collections.Generic;

namespace PostItNoteRacing.Plugin.Models
{
    /// <summary>
    /// Utility class, make sure it can be correctly serialized using JSON.net.
    /// </summary>
    internal class Utility
    {
        public int BooleanQuantity { get; set; } = 0;

        public List<IntegerProperty> IntegerActions { get; set; } = new List<IntegerProperty>();
    }
}