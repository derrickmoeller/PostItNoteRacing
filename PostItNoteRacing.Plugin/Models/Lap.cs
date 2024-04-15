using System;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Lap
    {
        public Lap(int number)
        {
            Number = number;
        }

        public bool IsInLap { get; set; }

        public bool IsOutLap { get; set; }

        public bool IsValid { get; set; }

        public MiniSector LastMiniSector => MiniSectors.OrderByDescending(x => x.TrackPosition).FirstOrDefault();

        public List<MiniSector> MiniSectors { get; } = new List<MiniSector>();

        public int Number { get; }

        public TimeSpan Time => LastMiniSector?.Time ?? TimeSpan.Zero;
    }
}