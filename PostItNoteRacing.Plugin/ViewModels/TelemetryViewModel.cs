using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.Properties;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class TelemetryViewModel : SettingsViewModel<Telemetry>
    {
        private readonly Session _session;

        public TelemetryViewModel(IModifySimHub plugin)
            : base(plugin, Resources.TelemetryViewModel_DisplayName)
        {
            if (EnableTelemetry == true)
            {
                _session = new Session(Plugin, this);
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

        public bool EnableInverseGapStrings
        {
            get => Entity.EnableInverseGapStrings;
            set
            {
                if (Entity.EnableInverseGapStrings != value)
                {
                    Entity.EnableInverseGapStrings = value;
                    NotifyPropertyChanged();
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

        public bool UseLastNLapsToEstimateLapTime
        {
            get => Entity.UseLastNLapsToEstimateLapTime;
            set
            {
                if (Entity.UseLastNLapsToEstimateLapTime != value)
                {
                    Entity.UseLastNLapsToEstimateLapTime = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _session?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}