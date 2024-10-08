﻿using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.Properties;
using PostItNoteRacing.Plugin.Telemetry;
using SimHub.Plugins;
using System;
using System.Linq;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class TelemetryViewModel : SettingsViewModel<Models.Telemetry>, IProvideSettings
    {
        private Session _session;

        public TelemetryViewModel(IModifySimHub plugin)
            : base(plugin, Resources.TelemetryViewModel_DisplayName)
        {
            Plugin.DataUpdated += OnPluginDataUpdated;

            AddActions();
            AttachDelegates();

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

        public int XLaps
        {
            get => Entity.XLaps;
            set
            {
                if (Entity.XLaps != value)
                {
                    if (value < XLapsMinimum)
                    {
                        Entity.XLaps = XLapsMaximum;
                    }
                    else if (value > XLapsMaximum)
                    {
                        Entity.XLaps = XLapsMinimum;
                    }
                    else
                    {
                        Entity.XLaps = value;
                    }

                    NotifyPropertyChanged();
                }
            }
        }

        public int XLapsMaximum { get; } = 50;

        public int XLapsMinimum { get; } = 1;

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

                TryDetachDelegates();

                Plugin.DataUpdated -= OnPluginDataUpdated;
                Session.DescriptionChanging -= OnSessionDescriptionChanging;
            }

            base.Dispose(disposing);
        }

        private void AddActions()
        {
            Plugin.AddAction("DecrementNLaps", (a, b) => NLaps--);
            Plugin.AddAction("DecrementXLaps", (a, b) => XLaps--);
            Plugin.AddAction("IncrementNLaps", (a, b) => NLaps++);
            Plugin.AddAction("IncrementXLaps", (a, b) => XLaps++);
            Plugin.AddAction("LastReferenceLap", (a, b) => ReferenceLap--);
            Plugin.AddAction("NextReferenceLap", (a, b) => ReferenceLap++);
            Plugin.AddAction("ResetBestLaps", ResetBestLaps);
        }

        private void AttachDelegates()
        {
            Plugin.AttachDelegate("Settings_NLaps", () => NLaps);
            Plugin.AttachDelegate("Settings_OverrideJavaScriptFunctions", () => OverrideJavaScriptFunctions);
            Plugin.AttachDelegate("Settings_ReferenceLap", () => ReferenceLap);
            Plugin.AttachDelegate("Settings_XLaps", () => XLaps);
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
            Session = null;
        }

        private void ResetBestLaps(PluginManager _, string __)
        {
            Session?.ResetBestLaps();
        }

        private void TryDetachDelegates()
        {
            Plugin?.DetachDelegate("Settings_NLaps");
            Plugin?.DetachDelegate("Settings_OverrideJavaScriptFunctions");
            Plugin?.DetachDelegate("Settings_ReferenceLap");
            Plugin?.DetachDelegate("Settings_XLaps");
        }
    }
}