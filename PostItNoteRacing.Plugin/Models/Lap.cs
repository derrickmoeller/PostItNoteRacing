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

            MiniSectors.Add(new MiniSector
            {
                Time = TimeSpan.Zero,
                TrackPosition = 0,
            });
        }

        public bool IsDirty { get; set; } = false;

        public bool IsInLap { get; set; } = false;

        public bool IsOutLap { get; set; }

        public MiniSector LastMiniSector => MiniSectors.OrderByDescending(x => x.TrackPosition).FirstOrDefault();

        public List<MiniSector> MiniSectors { get; } = new List<MiniSector>();

        public int Number { get; }

        public TimeSpan Time => LastMiniSector?.Time ?? TimeSpan.Zero;
    }
}