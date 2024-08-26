using PostItNoteRacing.Common.ViewModels;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.Properties;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class UtilityViewModel : SettingsViewModel<Utility>
    {
        private ObservableCollection<BooleanPropertyViewModel> _booleanActions;
        private ObservableCollection<IntegerPropertyViewModel> _integerActions;

        public UtilityViewModel(IModifySimHub plugin)
            : base(plugin, Resources.UtilityViewModel_DisplayName)
        {
            foreach (var action in Enumerable.Range(1, BooleanQuantity).Select(x => new BooleanPropertyViewModel(Plugin, x)))
            {
                BooleanActions.Add(action);
            }

            foreach (var action in Entity.IntegerActions.Select(x => new IntegerPropertyViewModel(Plugin, x)))
            {
                IntegerActions.Add(action);
            }
        }

        public ObservableCollection<BooleanPropertyViewModel> BooleanActions
        {
            get
            {
                if (_booleanActions == null)
                {
                    _booleanActions = new ObservableCollection<BooleanPropertyViewModel>();
                    _booleanActions.CollectionChanged += OnBooleanActionsCollectionChanged;
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

        public ObservableCollection<IntegerPropertyViewModel> IntegerActions
        {
            get
            {
                if (_integerActions == null)
                {
                    _integerActions = new ObservableCollection<IntegerPropertyViewModel>();
                    _integerActions.CollectionChanged += OnIntegerActionsCollectionChanged;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_booleanActions != null)
                {
                    foreach (var action in _booleanActions)
                    {
                        action.Dispose();
                    }

                    _booleanActions.CollectionChanged -= OnBooleanActionsCollectionChanged;
                }

                if (_integerActions != null)
                {
                    foreach (var action in _integerActions)
                    {
                        action.PropertyChanged -= OnIntegerActionPropertyChanged;
                        action.Dispose();
                    }

                    _integerActions.CollectionChanged -= OnIntegerActionsCollectionChanged;
                }
            }

            base.Dispose(disposing);
        }

        private void OnBooleanActionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (ViewModelBase viewModel in e.OldItems)
                {
                    viewModel.Dispose();
                }
            }
        }

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
                foreach (var action in Enumerable.Range(BooleanActions.Count + 1, BooleanQuantity - BooleanActions.Count).Select(x => new BooleanPropertyViewModel(Plugin, x)))
                {
                    BooleanActions.Add(action);
                }
            }

            NotifyPropertyChanged(nameof(BooleanQuantity));
        }

        private void OnIntegerActionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(IntegerActions));
        }

        private void OnIntegerActionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Entity.IntegerActions = IntegerActions.Select(x => x.Entity).ToList();

            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (ViewModelBase viewModel in e.OldItems)
                {
                    viewModel.PropertyChanged -= OnIntegerActionPropertyChanged;
                    viewModel.Dispose();
                }
            }

            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (ViewModelBase viewModel in e.NewItems)
                {
                    viewModel.PropertyChanged += OnIntegerActionPropertyChanged;
                }
            }

            NotifyPropertyChanged(nameof(IntegerActions));
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
                foreach (var action in Enumerable.Range(IntegerActions.Count + 1, quantity - IntegerActions.Count).Select(x => new IntegerPropertyViewModel(Plugin, new IntegerProperty(x))))
                {
                    IntegerActions.Add(action);
                }
            }

            NotifyPropertyChanged(nameof(IntegerQuantity));
        }
    }
}