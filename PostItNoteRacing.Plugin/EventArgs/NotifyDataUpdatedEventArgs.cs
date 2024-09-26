using GameReaderCommon;

namespace PostItNoteRacing.Plugin.EventArgs
{
    internal class NotifyDataUpdatedEventArgs(GameData data, bool isLicensed)
    {
        public GameData Data { get; } = data;

        public bool IsLicensed { get; } = isLicensed;
    }
}
