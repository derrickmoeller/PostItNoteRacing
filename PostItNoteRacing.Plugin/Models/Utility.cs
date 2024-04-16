using PostItNoteRacing.Plugin.Interfaces;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Utility
    {
        private readonly IModifySimHub _modifySimHub;
        private readonly Settings _settings;

        public Utility(IModifySimHub modifySimHub, Settings settings)
        {
            _modifySimHub = modifySimHub;
            _settings = settings;

            CreateSimHubActions();
            CreateSimHubProperties();
        }

        private void CreateSimHubActions()
        {
            // Boolean properties.
            _modifySimHub.AddAction("ToggleBooleanA", (a, b) => _modifySimHub.SetProperty("Booleans_A", !_modifySimHub.GetProperty("Booleans_A")));
            _modifySimHub.AddAction("ToggleBooleanB", (a, b) => _modifySimHub.SetProperty("Booleans_B", !_modifySimHub.GetProperty("Booleans_B")));
            _modifySimHub.AddAction("ToggleBooleanC", (a, b) => _modifySimHub.SetProperty("Booleans_C", !_modifySimHub.GetProperty("Booleans_C")));
            _modifySimHub.AddAction("ToggleBooleanD", (a, b) => _modifySimHub.SetProperty("Booleans_D", !_modifySimHub.GetProperty("Booleans_D")));

            // Integer properties.
            _modifySimHub.AddAction("IncrementIntegerA", (a, b) => IncrementInteger("Integers_A", _settings.IntegerAMax));
            _modifySimHub.AddAction("IncrementIntegerB", (a, b) => IncrementInteger("Integers_A", _settings.IntegerBMax));
            _modifySimHub.AddAction("IncrementIntegerC", (a, b) => IncrementInteger("Integers_A", _settings.IntegerCMax));
            _modifySimHub.AddAction("IncrementIntegerD", (a, b) => IncrementInteger("Integers_A", _settings.IntegerDMax));
            _modifySimHub.AddAction("DecrementIntegerA", (a, b) => DecrementInteger("Integers_A", _settings.IntegerAMax));
            _modifySimHub.AddAction("DecrementIntegerB", (a, b) => DecrementInteger("Integers_B", _settings.IntegerBMax));
            _modifySimHub.AddAction("DecrementIntegerC", (a, b) => DecrementInteger("Integers_C", _settings.IntegerCMax));
            _modifySimHub.AddAction("DecrementIntegerD", (a, b) => DecrementInteger("Integers_D", _settings.IntegerDMax));
        }

        private void CreateSimHubProperties()
        {
            _modifySimHub.AddProperty("Booleans_A", false);
            _modifySimHub.AddProperty("Booleans_B", false);
            _modifySimHub.AddProperty("Booleans_C", false);
            _modifySimHub.AddProperty("Booleans_D", false);
            _modifySimHub.AddProperty("Integers_A", 1);
            _modifySimHub.AddProperty("Integers_B", 1);
            _modifySimHub.AddProperty("Integers_C", 1);
            _modifySimHub.AddProperty("Integers_D", 1);
        }

        private void DecrementInteger(string propertyName, int maxValue)
        {
            int value = _modifySimHub.GetProperty(propertyName);
            if (--value < 1)
            {
                value = maxValue;
            }

            _modifySimHub.SetProperty(propertyName, value);
        }

        private void IncrementInteger(string propertyName, int maxValue)
        {
            int value = _modifySimHub.GetProperty(propertyName);
            if (++value > maxValue)
            {
                value = 1;
            }

            _modifySimHub.SetProperty(propertyName, value);
        }
    }
}