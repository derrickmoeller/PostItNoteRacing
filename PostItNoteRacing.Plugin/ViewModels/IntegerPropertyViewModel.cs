using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class IntegerPropertyViewModel : ViewModelBase
    {
        public IntegerPropertyViewModel(IModifySimHub modifySimHub, IntegerProperty entity)
            : base(modifySimHub)
        {
            Entity = entity;

            DecrementInteger = new SimHubAction($"PostItNoteRacing.Decrement_Integer_{Id:D2}", $"Decrement Integer {Id:D2}");
            IncrementInteger = new SimHubAction($"PostItNoteRacing.Increment_Integer_{Id:D2}", $"Increment Integer {Id:D2}");

            CreateActions();
            CreateProperties();
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

        private void CreateActions()
        {
            ModifySimHub.AddAction($"Increment_Integer_{Id:D2}", (a, b) => Increment($"Integers_{Id:D2}"));
            ModifySimHub.AddAction($"Decrement_Integer_{Id:D2}", (a, b) => Decrement($"Integers_{Id:D2}"));
        }

        private void CreateProperties()
        {
            ModifySimHub.AddProperty($"Integers_{Id:D2}", Minimum);
        }

        private void Decrement(string propertyName)
        {
            int value = ModifySimHub.GetProperty(propertyName);
            if (--value < Minimum)
            {
                value = Maximum;
            }

            ModifySimHub.SetProperty(propertyName, value);
        }

        private void Increment(string propertyName)
        {
            int value = ModifySimHub.GetProperty(propertyName);
            if (++value > Maximum)
            {
                value = Minimum;
            }

            ModifySimHub.SetProperty(propertyName, value);
        }
    }
}