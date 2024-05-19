using PostItNoteRacing.Common.ViewModels;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class BooleanPropertyViewModel : ViewModelBase
    {
        public BooleanPropertyViewModel(IModifySimHub plugin, int id)
        {
            ToggleBoolean = new SimHubAction($"PostItNoteRacing.Toggle_Boolean_{id:D2}", $"Toggle Boolean {id:D2}");

            plugin.AddAction($"Toggle_Boolean_{id:D2}", (a, b) => plugin.SetProperty($"Booleans_{id:D2}", plugin.GetProperty($"Booleans_{id:D2}") == false));
            plugin.AddProperty($"Booleans_{id:D2}", false);
        }

        public SimHubAction ToggleBoolean { get; }
    }
}