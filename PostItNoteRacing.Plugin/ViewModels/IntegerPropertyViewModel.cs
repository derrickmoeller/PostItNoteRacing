using PostItNoteRacing.Common.ViewModels;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class IntegerPropertyViewModel : ViewModelBase
    {
        public IntegerPropertyViewModel(IModifySimHub plugin, IntegerProperty entity)
        {
            Entity = entity;

            DecrementInteger = new SimHubAction($"PostItNoteRacing.Decrement_Integer_{Id:D2}", $"Decrement Integer {Id:D2}");
            IncrementInteger = new SimHubAction($"PostItNoteRacing.Increment_Integer_{Id:D2}", $"Increment Integer {Id:D2}");

            CreateActions(plugin);
            CreateProperties(plugin);
        }

        public SimHubAction DecrementInteger { get; }

        public int Id => Entity.Id;

        public SimHubAction IncrementInteger { get; }

        public int Maximum
        {
            get => Entity.Maximum;
            set
            {
                if (Entity.Maximum != value)
                {
                    Entity.Maximum = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Minimum
        {
            get => Entity.Minimum;
            set
            {
                if (Entity.Minimum != value)
                {
                    Entity.Minimum = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal IntegerProperty Entity { get; }

        private void CreateActions(IModifySimHub plugin)
        {
            plugin.AddAction($"Increment_Integer_{Id:D2}", (a, b) => Increment($"Integers_{Id:D2}"));
            plugin.AddAction($"Decrement_Integer_{Id:D2}", (a, b) => Decrement($"Integers_{Id:D2}"));

            void Decrement(string propertyName)
            {
                int value = plugin.GetProperty(propertyName);
                if (--value < Minimum)
                {
                    value = Maximum;
                }

                plugin.SetProperty(propertyName, value);
            }

            void Increment(string propertyName)
            {
                int value = plugin.GetProperty(propertyName);
                if (++value > Maximum)
                {
                    value = Minimum;
                }

                plugin.SetProperty(propertyName, value);
            }
        }

        private void CreateProperties(IModifySimHub plugin)
        {
            plugin.AddProperty($"Integers_{Id:D2}", Minimum);
        }
    }
}