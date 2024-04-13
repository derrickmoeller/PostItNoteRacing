using System;

namespace PostItNoteRacing.Plugin.EventArgs
{
    internal class BestLapChangedEventArgs
    {
        public BestLapChangedEventArgs(TimeSpan? lapTime)
        {
            LapTime = lapTime;
        }

        public TimeSpan? LapTime { get; }
    }
}
