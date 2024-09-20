using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.Properties;
using PostItNoteRacing.Plugin.Telemetry;
using System;
using System.Linq;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class TelemetryViewModel : SettingsViewModel<Models.Telemetry>
    {
        private Session _session;

        public TelemetryViewModel(IModifySimHub plugin)
            : base(plugin, Resources.TelemetryViewModel_DisplayName)
        {
            Plugin.AddAction("DecrementNLaps", (a, b) => NLaps--);
            Plugin.AddAction("IncrementNLaps", (a, b) => NLaps++);
            Plugin.AddAction("LastReferenceLap", (a, b) => ReferenceLap--);
            Plugin.AddAction("NextReferenceLap", (a, b) => ReferenceLap++);

            Plugin.AttachDelegate("Settings_NLaps", () => NLaps);
            Plugin.AttachDelegate("Settings_OverrideJavaScriptFunctions", () => OverrideJavaScriptFunctions);
            Plugin.AttachDelegate("Settings_ReferenceLap", () => ReferenceLap);

            Plugin.DataUpdated += OnPluginDataUpdated;
            Session.DescriptionChanging += OnSessionDescriptionChanging;
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

        private Session Session
        {
            get => _session;
            set
            {
                if (_session != value)
                {
                    _session?.Dispose();
                    _session = value;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Session?.Dispose();

                Plugin.DetachDelegate("Settings_NLaps");
                Plugin.DetachDelegate("Settings_OverrideJavaScriptFunctions");
                Plugin.DetachDelegate("Settings_ReferenceLap");

                Plugin.DataUpdated -= OnPluginDataUpdated;
                Session.DescriptionChanging -= OnSessionDescriptionChanging;
            }

            base.Dispose(disposing);
        }

        private void OnPluginDataUpdated(object sender, NotifyDataUpdatedEventArgs e)
        {
            if (e.Data.GameRunning && e.Data.NewData != null)
            {
                Session ??= new Session(Plugin, this);
            }
            else
            {
                Session = null;
            }
        }

        private void OnSessionDescriptionChanging(object sender, System.EventArgs e)
        {
            Session = new Session(Plugin, this);
        }
    }
}