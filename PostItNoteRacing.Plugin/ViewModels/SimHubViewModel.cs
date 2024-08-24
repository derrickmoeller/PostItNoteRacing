using PostItNoteRacing.Common.ViewModels;
using PostItNoteRacing.Plugin.Interfaces;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal abstract class SimHubViewModel(IModifySimHub plugin, string displayName = null) : ViewModelBase(displayName)
    {
        protected IModifySimHub Plugin { get; } = plugin;
    }
}