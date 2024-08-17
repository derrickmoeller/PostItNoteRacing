using GameReaderCommon;

namespace PostItNoteRacing.Plugin.EventArgs
{
    internal class NotifyDataUpdatedEventArgs(GameData data)
    {
        public GameData Data { get; } = data;
    }
}
