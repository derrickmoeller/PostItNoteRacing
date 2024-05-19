using PostItNoteRacing.Common.ViewModels;
using PostItNoteRacing.Plugin.Interfaces;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal abstract class SettingsViewModel<T> : ViewModelBase
     where T : new()
    {
        public SettingsViewModel(IModifySimHub plugin, string displayName)
            : base(displayName)
        {
            Plugin = plugin;

            Entity = Plugin.ReadSettings(typeof(T).Name, () => new T());
        }

        protected T Entity { get; }

        protected IModifySimHub Plugin { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Plugin.SaveSettings(typeof(T).Name, Entity);
            }

            base.Dispose(disposing);
        }
    }
}
