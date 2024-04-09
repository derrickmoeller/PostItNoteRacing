using System;

namespace PostItNoteRacing.Plugin.EventArgs
{
    internal class LapChangedEventArgs : System.EventArgs
    {
        public TimeSpan? LapTime { get; }

        public LapChangedEventArgs(TimeSpan? lapTime)
        {
            LapTime = lapTime;
        }
    }
}
