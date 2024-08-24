using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class BooleanPropertyViewModel : PropertyViewModel<bool>
    {
        private readonly int _id;

        public BooleanPropertyViewModel(IModifySimHub plugin, int id)
            : base(plugin)
        {
            _id = id;

            ToggleBoolean = new SimHubAction($"PostItNoteRacing.Toggle_Boolean_{_id:D2}", $"Toggle Boolean {_id:D2}");
            Plugin.AddAction($"Toggle_Boolean_{_id:D2}", (a, b) => Value = Value == false);

            Plugin.AttachDelegate($"Booleans_{_id:D2}", () => Value);
        }

        public SimHubAction ToggleBoolean { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Plugin.DetachDelegate($"Booleans_{_id:D2}");
            }

            base.Dispose(disposing);
        }
    }
}