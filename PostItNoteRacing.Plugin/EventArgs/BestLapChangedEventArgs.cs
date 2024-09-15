using PostItNoteRacing.Plugin.Telemetry;

namespace PostItNoteRacing.Plugin.EventArgs
{
    internal class BestLapChangedEventArgs
    {
        public BestLapChangedEventArgs(Lap lap)
        {
            Lap = lap;
        }

        public Lap Lap { get; }
    }
}
