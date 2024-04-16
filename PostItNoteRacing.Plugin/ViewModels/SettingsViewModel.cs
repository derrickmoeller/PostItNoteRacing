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

        public bool EnableTelemetry
        {
            get => _settings.EnableTelemetry;
            set
            {
                if (_settings.EnableTelemetry != value)
                {
                    _settings.EnableTelemetry = value;
                    OnPropertyChanged(nameof(EnableTelemetry));
                }
            }
        }

        public bool EnableUtility
        {
            get => _settings.EnableUtility;
            set
            {
                if (_settings.EnableUtility != value)
                {
                    _settings.EnableUtility = value;
                    OnPropertyChanged(nameof(EnableUtility));
                }
            }
        }

        public int IntegerAMax
        {
            get => _settings.IntegerAMax;
            set
            {
                if (_settings.IntegerAMax != value)
                {
                    _settings.IntegerAMax = value;
                    OnPropertyChanged(nameof(IntegerAMax));
                }
            }
        }

        public int IntegerBMax
        {
            get => _settings.IntegerBMax;
            set
            {
                if (_settings.IntegerBMax != value)
                {
                    _settings.IntegerBMax = value;
                    OnPropertyChanged(nameof(IntegerBMax));
                }
            }
        }

        public int IntegerCMax
        {
            get => _settings.IntegerCMax;
            set
            {
                if (_settings.IntegerCMax != value)
                {
                    _settings.IntegerCMax = value;
                    OnPropertyChanged(nameof(IntegerCMax));
                }
            }
        }

        public int IntegerDMax
        {
            get => _settings.IntegerDMax;
            set
            {
                if (_settings.IntegerDMax != value)
                {
                    _settings.IntegerDMax = value;
                    OnPropertyChanged(nameof(IntegerDMax));
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