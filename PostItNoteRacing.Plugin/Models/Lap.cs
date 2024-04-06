using System;
using System.Collections.Generic;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Lap
    {
        public bool IsInLap { get; set; }

        public bool IsOutLap { get; set; }

        public List<MiniSector> MiniSectors { get; } = new List<MiniSector>();

        public int Number { get; }

        public TimeSpan Time { get; set; }

        public Lap(int number)
        {
            Number = number;
        }
    }
}
