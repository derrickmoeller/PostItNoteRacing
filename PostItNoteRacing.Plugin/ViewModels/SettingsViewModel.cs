using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        private ObservableCollection<BooleanPropertyViewModel> _booleanActions;
        private ObservableCollection<IntegerPropertyViewModel> _integerActions;

        public SettingsViewModel(IModifySimHub modifySimHub, Settings settings)
            : base(modifySimHub)
        {
            Entity = settings;
        }

        public ObservableCollection<BooleanPropertyViewModel> BooleanActions
        {
            get
            {
                if (_booleanActions == null)
                {
                    _booleanActions = new ObservableCollection<BooleanPropertyViewModel>(Enumerable.Range(1, BooleanQuantity).Select(x => new BooleanPropertyViewModel(ModifySimHub, x)));
                }

                return _booleanActions;
            }
        }

        public int BooleanQuantity
        {
            get => Entity.BooleanQuantity;
            set
            {
                if (Entity.BooleanQuantity != value)
                {
                    Entity.BooleanQuantity = value;
                    OnBooleanQuantityChanged();
                }
            }
        }

        public bool EnableEstimatedLapTimes
        {
            get => Entity.EnableEstimatedLapTimes;
            set
            {
                if (Entity.EnableEstimatedLapTimes != value)
                {
                    Entity.EnableEstimatedLapTimes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool EnableGapCalculations
        {
            get => Entity.EnableGapCalculations;
            set
            {
                if (Entity.EnableGapCalculations != value)
                {
                    Entity.EnableGapCalculations = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool EnableTelemetry
        {
            get => Entity.EnableTelemetry;
            set
            {
                if (Entity.EnableTelemetry != value)
                {
                    Entity.EnableTelemetry = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool EnableUtility
        {
            get => Entity.EnableUtility;
            set
            {
                if (Entity.EnableUtility != value)
                {
                    Entity.EnableUtility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<IntegerPropertyViewModel> IntegerActions
        {
            get
            {
                if (_integerActions == null)
                {
                    _integerActions = new ObservableCollection<IntegerPropertyViewModel>(Entity.IntegerActions.Select(x => new IntegerPropertyViewModel(ModifySimHub, x)));
                }

                return _integerActions;
            }
        }

        public int IntegerQuantity
        {
            get => IntegerActions.Count;
            set
            {
                if (IntegerActions.Count != value)
                {
                    OnIntegerQuantityChanging(value);
                }
            }
        }

        public int NLaps
        {
            get => Entity.NLaps;
            set
            {
                if (Entity.NLaps != value)
                {
                    if (value < NLapsMinimum)
                    {
                        Entity.NLaps = NLapsMaximum;
                    }
                    else if (value > NLapsMaximum)
                    {
                        Entity.NLaps = NLapsMinimum;
                    }
                    else
                    {
                        Entity.NLaps = value;
                    }

                    NotifyPropertyChanged();
                }
            }
        }

        public int NLapsMaximum { get; } = 50;

        public int NLapsMinimum { get; } = 2;

        internal Settings Entity { get; }

        private void OnBooleanQuantityChanged()
        {
            if (BooleanActions.Count > BooleanQuantity)
            {
                foreach (var action in BooleanActions.Skip(BooleanQuantity).ToList())
                {
                    BooleanActions.Remove(action);
                }
            }
            else if (BooleanActions.Count < BooleanQuantity)
            {
                foreach (var action in Enumerable.Range(BooleanActions.Count + 1, BooleanQuantity - BooleanActions.Count).Select(x => new BooleanPropertyViewModel(ModifySimHub, x)))
                {
                    BooleanActions.Add(action);
                }
            }

            NotifyPropertyChanged(nameof(BooleanQuantity));
        }

        private void OnIntegerQuantityChanging(int quantity)
        {
            if (IntegerActions.Count > quantity)
            {
                foreach (var action in IntegerActions.Skip(quantity).ToList())
                {
                    IntegerActions.Remove(action);
                }
            }
            else if (IntegerActions.Count < quantity)
            {
                foreach (var action in Enumerable.Range(IntegerActions.Count + 1, quantity - IntegerActions.Count).Select(x => new IntegerPropertyViewModel(ModifySimHub, new IntegerProperty(x))))
                {
                    IntegerActions.Add(action);
                }
            }

            Entity.IntegerActions = IntegerActions.Select(x => x.Entity).ToList();

            NotifyPropertyChanged(nameof(IntegerQuantity));
        }
    }
}