using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class BooleanPropertyViewModel : ViewModelBase
    {
        public BooleanPropertyViewModel(IModifySimHub modifySimHub, int id)
            : base(modifySimHub)
        {
            ToggleBoolean = new SimHubAction($"PostItNoteRacing.Toggle_Boolean_{id:D2}", $"Toggle Boolean {id:D2}");

            ModifySimHub.AddAction($"Toggle_Boolean_{id:D2}", (a, b) => ModifySimHub.SetProperty($"Booleans_{id:D2}", modifySimHub.GetProperty($"Booleans_{id:D2}") == false));
            ModifySimHub.AddProperty($"Booleans_{id:D2}", false);
        }

        public SimHubAction ToggleBoolean { get; }
    }
}