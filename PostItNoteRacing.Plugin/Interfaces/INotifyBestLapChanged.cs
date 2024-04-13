using PostItNoteRacing.Plugin.EventArgs;
using System;

namespace PostItNoteRacing.Plugin.Interfaces
{
    internal interface INotifyBestLapChanged
    {
        event EventHandler<BestLapChangedEventArgs> BestLapChanged;
    }
}
