using PostItNoteRacing.Plugin.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel(Settings settings)
        {
            Entity = settings;
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

        public int IntegerAMax
        {
            get => Entity.IntegerAMax;
            set
            {
                if (Entity.IntegerAMax != value)
                {
                    Entity.IntegerAMax = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int IntegerBMax
        {
            get => Entity.IntegerBMax;
            set
            {
                if (Entity.IntegerBMax != value)
                {
                    Entity.IntegerBMax = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int IntegerCMax
        {
            get => Entity.IntegerCMax;
            set
            {
                if (Entity.IntegerCMax != value)
                {
                    Entity.IntegerCMax = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int IntegerDMax
        {
            get => Entity.IntegerDMax;
            set
            {
                if (Entity.IntegerDMax != value)
                {
                    Entity.IntegerDMax = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int LastNLaps
        {
            get => Entity.LastNLaps;
            set
            {
                if (Entity.LastNLaps != value)
                {
                    if (value < 2)
                    {
                        Entity.LastNLaps = 100;
                    }
                    else if (value > 100)
                    {
                        Entity.LastNLaps = 2;
                    }
                    else
                    {
                        Entity.LastNLaps = value;
                    }

                    NotifyPropertyChanged();
                }
            }
        }

        internal Settings Entity { get; }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Interface: INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}