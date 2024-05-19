using GameReaderCommon;

namespace PostItNoteRacing.Plugin.EventArgs
{
    internal class NotifyDataUpdatedEventArgs
    {
        public NotifyDataUpdatedEventArgs(GameData data)
        {
            Data = data;
        }

        public GameData Data { get; }
    }
}
