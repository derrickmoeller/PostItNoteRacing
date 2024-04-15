using PostItNoteRacing.Plugin.Models;
using System.ComponentModel;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly Settings _settings;

        public SettingsViewModel(Settings settings)
        {
            _settings = settings;
        }

        public bool EnableEstimatedLaps
        {
            get => _settings.EnableEstimatedLaps;
            set
            {
                if (_settings.EnableEstimatedLaps != value)
                {
                    _settings.EnableEstimatedLaps = value;
                    OnPropertyChanged(nameof(EnableEstimatedLaps));
                }
            }
        }

        public bool EnableRealGaps
        {
            get => _settings.EnableRealGaps;
            set
            {
                if (_settings.EnableRealGaps != value)
                {
                    _settings.EnableRealGaps = value;
                    OnPropertyChanged(nameof(EnableRealGaps));
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Interface: INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
