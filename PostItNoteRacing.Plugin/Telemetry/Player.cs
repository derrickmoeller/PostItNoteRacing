using GameReaderCommon;
using IRacingReader;
using PostItNoteRacing.Common;
using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Plugin.Telemetry
{
    internal class Player : DisposableObject
    {
        private readonly CornerTelemetry _brakeTemperatures;
        private readonly CornerTelemetry _tirePressures;
        private readonly CornerTelemetry _tireTemperatures;

        private readonly IModifySimHub _plugin;
        private readonly IProvideSettings _settingsProvider;

        private int _counter;
        private StatusDataBase _statusDatabase;

        public Player(IModifySimHub plugin, IProvideSettings settingsProvider)
        {
            _plugin = plugin;
            _plugin.DataUpdated += OnPluginDataUpdated;

            _settingsProvider = settingsProvider;

            _brakeTemperatures = new CornerTelemetry(_plugin, _settingsProvider, "BrakeTemperature");
            _tirePressures = new CornerTelemetry(_plugin, _settingsProvider, "TirePressure");
            _tireTemperatures = new CornerTelemetry(_plugin, _settingsProvider, "TireTemperature");

            AttachDelegates();
        }

        private int Incidents { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_plugin != null)
                {
                    _plugin.DataUpdated -= OnPluginDataUpdated;
                }

                _brakeTemperatures?.Dispose();
                _tirePressures?.Dispose();
                _tireTemperatures?.Dispose();

                TryDetachDelegates();
            }

            base.Dispose(disposing);
        }

        private void AttachDelegates()
        {
            _plugin.AttachDelegate($"Player_{nameof(Incidents)}", () => Incidents);
        }

        private void GetBrakeTemperatures()
        {
            _brakeTemperatures.AddValues(
                _statusDatabase.CurrentLap + _statusDatabase.TrackPositionPercent,
                _statusDatabase.BrakeTemperatureFrontLeft,
                _statusDatabase.BrakeTemperatureFrontRight,
                _statusDatabase.BrakeTemperatureRearLeft,
                _statusDatabase.BrakeTemperatureRearRight);
        }

        private void GetIncidents()
        {
            if (_statusDatabase.GetRawDataObject() is DataSampleEx iRacingData)
            {
                iRacingData.Telemetry.TryGetValue("PlayerCarTeamIncidentCount", out object rawIncidents);

                Incidents = Convert.ToInt32(rawIncidents);
            }
        }

        private void GetTirePressures()
        {
            _tirePressures.AddValues(
                _statusDatabase.CurrentLap + _statusDatabase.TrackPositionPercent,
                _statusDatabase.TyrePressureFrontLeft,
                _statusDatabase.TyrePressureFrontRight,
                _statusDatabase.TyrePressureRearLeft,
                _statusDatabase.TyrePressureRearRight);
        }

        private void GetTireTemperatures()
        {
            _tireTemperatures.AddValues(
                _statusDatabase.CurrentLap + _statusDatabase.TrackPositionPercent,
                _statusDatabase.TyreTemperatureFrontLeft,
                _statusDatabase.TyreTemperatureFrontRight,
                _statusDatabase.TyreTemperatureRearLeft,
                _statusDatabase.TyreTemperatureRearRight);
        }

        private void OnPluginDataUpdated(object sender, NotifyDataUpdatedEventArgs e)
        {
            _counter++;

            if (OnTrack(e.Data) == true)
            {
                _statusDatabase = e.Data.NewData;

                if (e.IsLicensed == true) // 60Hz
                {
                    if (_counter > 179)
                    {
                        _counter = 0;
                    }

                    // 0
                    if (_counter % 180 == 0)
                    {
                        GetBrakeTemperatures();
                    }

                    // 60
                    if (_counter % 180 == 60)
                    {
                        GetTirePressures();
                    }

                    // 120
                    if (_counter % 180 == 120)
                    {
                        GetTireTemperatures();
                    }

                    // 0, 60, 120
                    if (_counter % 60 == 0)
                    {
                        GetIncidents();
                    }
                }
                else // 10Hz
                {
                    if (_counter > 29)
                    {
                        _counter = 0;
                    }

                    // 0
                    if (_counter % 30 == 0)
                    {
                        GetBrakeTemperatures();
                    }

                    // 10
                    if (_counter % 30 == 10)
                    {
                        GetTirePressures();
                    }

                    // 20
                    if (_counter % 30 == 20)
                    {
                        GetTireTemperatures();
                    }

                    // 0, 10, 20
                    if (_counter % 10 == 0)
                    {
                        GetIncidents();
                    }
                }
            }
        }

        private bool OnTrack(GameData data)
        {
            return data.GameRunning &&
                data.NewData != null &&
                data.GameInMenu == false &&
                data.GamePaused == false &&
                data.NewData.IsInPitLane == 0 &&
                data.NewData.IsInPit == 0;
        }

        private void TryDetachDelegates()
        {
            _plugin?.DetachDelegate($"Player_{nameof(Incidents)}");
        }

        private class CornerTelemetry : DisposableObject
        {
            private readonly string _description;
            private readonly IModifySimHub _plugin;
            private readonly IProvideSettings _settingsProvider;
            private readonly Queue<(double LapHighPrecision, double FrontLeft, double FrontRight, double RearLeft, double RearRight)> _values = new ();

            public CornerTelemetry(IModifySimHub plugin, IProvideSettings settingsProvider, string description)
            {
                _plugin = plugin;
                _settingsProvider = settingsProvider;
                _description = description;

                AttachDelegates();
            }

            private double FrontLeft => _values.LastOrDefault().FrontLeft;

            private double FrontLeftAverage { get; set; }

            private double FrontRight => _values.LastOrDefault().FrontRight;

            private double FrontRightAverage { get; set; }

            private double RearLeft => _values.LastOrDefault().RearLeft;

            private double RearLeftAverage { get;  set; }

            private double RearRight => _values.LastOrDefault().RearRight;

            private double RearRightAverage { get; set; }

            public void AddValues(double lapHighPrecision, double frontLeft, double frontRight, double rearLeft, double rearRight)
            {
                _values.Enqueue((lapHighPrecision, frontLeft, frontRight, rearLeft, rearRight));

                while (_values.Any(x => x.LapHighPrecision < lapHighPrecision - _settingsProvider.XLaps))
                {
                    _values.Dequeue();
                }

                FrontLeftAverage = _values.Average(x => x.FrontLeft);
                FrontRightAverage = _values.Average(x => x.FrontRight);
                RearLeftAverage = _values.Average(x => x.RearLeft);
                RearRightAverage = _values.Average(x => x.RearRight);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    TryDetachDelegates();
                }

                base.Dispose(disposing);
            }

            private void AttachDelegates()
            {
                _plugin.AttachDelegate($"Player_{_description}{nameof(FrontLeft)}", () => FrontLeft);
                _plugin.AttachDelegate($"Player_{_description}{nameof(FrontLeftAverage)}", () => FrontLeftAverage);
                _plugin.AttachDelegate($"Player_{_description}{nameof(FrontRight)}", () => FrontRight);
                _plugin.AttachDelegate($"Player_{_description}{nameof(FrontRightAverage)}", () => FrontRightAverage);
                _plugin.AttachDelegate($"Player_{_description}{nameof(RearLeft)}", () => RearLeft);
                _plugin.AttachDelegate($"Player_{_description}{nameof(RearLeftAverage)}", () => RearLeftAverage);
                _plugin.AttachDelegate($"Player_{_description}{nameof(RearRight)}", () => RearRight);
                _plugin.AttachDelegate($"Player_{_description}{nameof(RearRightAverage)}", () => RearRightAverage);
            }

            private void TryDetachDelegates()
            {
                _plugin?.DetachDelegate($"Player_{_description}{nameof(FrontLeft)}");
                _plugin?.DetachDelegate($"Player_{_description}{nameof(FrontLeftAverage)}");
                _plugin?.DetachDelegate($"Player_{_description}{nameof(FrontRight)}");
                _plugin?.DetachDelegate($"Player_{_description}{nameof(FrontRightAverage)}");
                _plugin?.DetachDelegate($"Player_{_description}{nameof(RearLeft)}");
                _plugin?.DetachDelegate($"Player_{_description}{nameof(RearLeftAverage)}");
                _plugin?.DetachDelegate($"Player_{_description}{nameof(RearRight)}");
                _plugin?.DetachDelegate($"Player_{_description}{nameof(RearRightAverage)}");
            }
        }
    }
}