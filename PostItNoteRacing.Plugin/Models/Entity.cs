using PostItNoteRacing.Common;
using PostItNoteRacing.Plugin.Interfaces;

namespace PostItNoteRacing.Plugin.Models
{
    internal abstract class Entity(int index, IModifySimHub plugin) : Disposable
    {
        public int Index { get; } = index;

        protected IModifySimHub Plugin { get; } = plugin;
    }
}
