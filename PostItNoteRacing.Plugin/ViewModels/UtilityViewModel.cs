using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.Properties;
using System.Collections.ObjectModel;
using System.Linq;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class UtilityViewModel : SettingsViewModel<Utility>
    {
        public UtilityViewModel(IModifySimHub plugin)
            : base(plugin, Resources.UtilityViewModel_DisplayName)
        {
            BooleanActions = new ObservableCollection<BooleanPropertyViewModel>(Enumerable.Range(1, BooleanQuantity).Select(x => new BooleanPropertyViewModel(Plugin, x)));
            IntegerActions = new ObservableCollection<IntegerPropertyViewModel>(Entity.IntegerActions.Select(x => new IntegerPropertyViewModel(Plugin, x)));
        }

        public ObservableCollection<BooleanPropertyViewModel> BooleanActions { get; }

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

        public ObservableCollection<IntegerPropertyViewModel> IntegerActions { get; }

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

        private void OnBooleanQuantityChanged()
        {
            if (BooleanActions.Count > BooleanQuantity)
            {
                foreach (var action in BooleanActions.Skip(BooleanQuantity).ToList())
                {
                    action.Dispose();
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

        private void OnIntegerQuantityChanging(int quantity)
        {
            if (IntegerActions.Count > quantity)
            {
                foreach (var action in IntegerActions.Skip(quantity).ToList())
                {
                    action.Dispose();
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

            Entity.IntegerActions = IntegerActions.Select(x => x.Entity).ToList();

            NotifyPropertyChanged(nameof(IntegerQuantity));
        }
    }
}