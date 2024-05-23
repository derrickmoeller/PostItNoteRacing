using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.Properties;
using System;
using System.Linq;

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

        public bool OverrideJavaScriptFunctions
        {
            get => Entity.OverrideJavaScriptFunctions;
            set
            {
                if (Entity.OverrideJavaScriptFunctions != value)
                {
                    Entity.OverrideJavaScriptFunctions = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ReferenceLap ReferenceLap
        {
            get => Entity.ReferenceLap;
            set
            {
                if (Entity.ReferenceLap != value)
                {
                    if (Enum.IsDefined(typeof(ReferenceLap), value))
                    {
                        Entity.ReferenceLap = value;
                    }
                    else if (Entity.ReferenceLap < value)
                    {
                        Entity.ReferenceLap = Enum.GetValues(typeof(ReferenceLap)).Cast<ReferenceLap>().Min();
                    }
                    else
                    {
                        Entity.ReferenceLap = Enum.GetValues(typeof(ReferenceLap)).Cast<ReferenceLap>().Max();
                    }

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