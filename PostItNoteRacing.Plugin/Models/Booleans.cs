using PostItNoteRacing.Plugin.Interfaces;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Booleans
    {
        private readonly IModifySimHub _modifySimHub;

        public Booleans(IModifySimHub modifySimHub)
        {
            _modifySimHub = modifySimHub;

            CreateSimHubActions();
            CreateSimHubProperties();
        }

        private void CreateSimHubActions()
        {
            _modifySimHub.AddAction("ToggleBooleanA", (a, b) => _modifySimHub.SetProperty("Booleans_A", !_modifySimHub.GetProperty("Booleans_A")));
            _modifySimHub.AddAction("ToggleBooleanB", (a, b) => _modifySimHub.SetProperty("Booleans_B", !_modifySimHub.GetProperty("Booleans_B")));
            _modifySimHub.AddAction("ToggleBooleanC", (a, b) => _modifySimHub.SetProperty("Booleans_C", !_modifySimHub.GetProperty("Booleans_C")));
            _modifySimHub.AddAction("ToggleBooleanD", (a, b) => _modifySimHub.SetProperty("Booleans_D", !_modifySimHub.GetProperty("Booleans_D")));
        }

        private void CreateSimHubProperties()
        {
            _modifySimHub.AddProperty("Booleans_A", false);
            _modifySimHub.AddProperty("Booleans_B", false);
            _modifySimHub.AddProperty("Booleans_C", false);
            _modifySimHub.AddProperty("Booleans_D", false);
        }
    }
}