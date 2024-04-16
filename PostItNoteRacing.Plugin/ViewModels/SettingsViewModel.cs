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

        public bool EnableBooleans
        {
            get => _settings.EnableBooleans;
            set
            {
                if (_settings.EnableBooleans != value)
                {
                    _settings.EnableBooleans = value;
                    OnPropertyChanged(nameof(EnableBooleans));
                }
            }
        }

        public bool EnableEstimatedLapTimes
        {
            get => _settings.EnableEstimatedLapTimes;
            set
            {
                if (_settings.EnableEstimatedLapTimes != value)
                {
                    _settings.EnableEstimatedLapTimes = value;
                    OnPropertyChanged(nameof(EnableEstimatedLapTimes));
                }
            }
        }

        public bool EnableExtraProperties
        {
            get => _settings.EnableExtraProperties;
            set
            {
                if (_settings.EnableExtraProperties != value)
                {
                    _settings.EnableExtraProperties = value;
                    OnPropertyChanged(nameof(EnableExtraProperties));
                }
            }
        }

        public bool EnableGapCalculations
        {
            get => _settings.EnableGapCalculations;
            set
            {
                if (_settings.EnableGapCalculations != value)
                {
                    _settings.EnableGapCalculations = value;
                    OnPropertyChanged(nameof(EnableGapCalculations));
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
