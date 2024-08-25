using PostItNoteRacing.Common;
using PostItNoteRacing.Plugin.Interfaces;

namespace PostItNoteRacing.Plugin.Models
{
    internal abstract class Entity : DisposableObject
    {
        public Entity(IModifySimHub plugin, int index = 1)
        {
            Index = index;
            Plugin = plugin;

            AttachDelegates();
        }

        public int Index { get; }

        protected IModifySimHub Plugin { get; }

        protected abstract void AttachDelegates();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TryDetachDelegates();
            }

            base.Dispose(disposing);
        }

        protected abstract void TryDetachDelegates();
    }
}
