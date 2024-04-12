using System;

namespace PostItNoteRacing.Plugin.EventArgs
{
    internal class LapChangedEventArgs : System.EventArgs
    {
        public LapChangedEventArgs(TimeSpan? lapTime)
        {
            LapTime = lapTime;
        }

        public TimeSpan? LapTime { get; }
    }
}
