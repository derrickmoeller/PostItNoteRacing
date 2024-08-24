using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class IntegerPropertyViewModel : PropertyViewModel<int>
    {
        public IntegerPropertyViewModel(IModifySimHub plugin, IntegerProperty entity)
            : base(plugin)
        {
            Entity = entity;

            Value = Minimum;

            DecrementInteger = new SimHubAction($"PostItNoteRacing.Decrement_Integer_{Id:D2}", $"Decrement Integer {Id:D2}");
            Plugin.AddAction($"Decrement_Integer_{Id:D2}", (a, b) => Value--);

            IncrementInteger = new SimHubAction($"PostItNoteRacing.Increment_Integer_{Id:D2}", $"Increment Integer {Id:D2}");
            Plugin.AddAction($"Increment_Integer_{Id:D2}", (a, b) => Value++);

            Plugin.AttachDelegate($"Integers_{Id:D2}", () => Value);
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

        protected override int Value
        {
            get => base.Value;
            set
            {
                if (Value != value)
                {
                    if (value < Minimum)
                    {
                        base.Value = Maximum;
                    }
                    else if (value > Maximum)
                    {
                        base.Value = Minimum;
                    }
                    else
                    {
                        base.Value = value;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Plugin.DetachDelegate($"Integers_{Id:D2}");
            }

            base.Dispose(disposing);
        }
    }
}