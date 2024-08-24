using PostItNoteRacing.Plugin.Interfaces;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal abstract class PropertyViewModel<T>(IModifySimHub plugin) : SimHubViewModel(plugin)
    {
        protected virtual T Value { get; set; } = default;
    }
}