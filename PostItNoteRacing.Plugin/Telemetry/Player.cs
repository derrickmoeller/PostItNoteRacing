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
        private readonly CornerTelemetry _brakeTemperatures = new ();
        private readonly CornerTelemetry _tirePressures = new ();
        private readonly CornerTelemetry _tireTemperatures = new ();
        private readonly IModifySimHub _plugin;

        private int _counter;

        public Player(IModifySimHub plugin)
        {
            _plugin = plugin;
            _plugin.DataUpdated += OnPluginDataUpdated;

            AttachDelegates();
        }

        private double BrakeTemperatureFrontLeft => _brakeTemperatures.FrontLeft;

        private double BrakeTemperatureFrontLeftAverage => _brakeTemperatures.FrontLeftAverage;

        private double BrakeTemperatureFrontRight => _brakeTemperatures.FrontRight;

        private double BrakeTemperatureFrontRightAverage => _brakeTemperatures.FrontRightAverage;

        private double BrakeTemperatureRearLeft => _brakeTemperatures.RearLeft;

        private double BrakeTemperatureRearLeftAverage => _brakeTemperatures.RearLeftAverage;

        private double BrakeTemperatureRearRight => _brakeTemperatures.RearRight;

        private double BrakeTemperatureRearRightAverage => _brakeTemperatures.RearRightAverage;

        private int Incidents { get; set; }

        private StatusDataBase StatusDatabase { get; set; }

        private double TirePressureFrontLeft => _tirePressures.FrontLeft;

        private double TirePressureFrontLeftAverage => _tirePressures.FrontLeftAverage;

        private double TirePressureFrontRight => _tirePressures.FrontRight;

        private double TirePressureFrontRightAverage => _tirePressures.FrontRightAverage;

        private double TirePressureRearLeft => _tirePressures.RearLeft;

        private double TirePressureRearLeftAverage => _tirePressures.RearLeftAverage;

        private double TirePressureRearRight => _tirePressures.RearRight;

        private double TirePressureRearRightAverage => _tirePressures.RearRightAverage;

        private double TireTemperatureFrontLeft => _tireTemperatures.FrontLeft;

        private double TireTemperatureFrontLeftAverage => _tireTemperatures.FrontLeftAverage;

        private double TireTemperatureFrontRight => _tireTemperatures.FrontRight;

        private double TireTemperatureFrontRightAverage => _tireTemperatures.FrontRightAverage;

        private double TireTemperatureRearLeft => _tireTemperatures.RearLeft;

        private double TireTemperatureRearLeftAverage => _tireTemperatures.RearLeftAverage;

        private double TireTemperatureRearRight => _tireTemperatures.RearRight;

        private double TireTemperatureRearRightAverage => _tireTemperatures.RearRightAverage;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_plugin != null)
                {
                    _plugin.DataUpdated -= OnPluginDataUpdated;
                }

                TryDetachDelegates();
            }

            base.Dispose(disposing);
        }

        private void AttachDelegates()
        {
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureFrontLeft)}", () => BrakeTemperatureFrontLeft);
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureFrontLeftAverage)}", () => BrakeTemperatureFrontLeftAverage);
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureFrontRight)}", () => BrakeTemperatureFrontRight);
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureFrontRightAverage)}", () => BrakeTemperatureFrontRightAverage);
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureRearLeft)}", () => BrakeTemperatureRearLeft);
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureRearLeftAverage)}", () => BrakeTemperatureRearLeftAverage);
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureRearRight)}", () => BrakeTemperatureRearRight);
            _plugin.AttachDelegate($"Player_{nameof(BrakeTemperatureRearRightAverage)}", () => BrakeTemperatureRearRightAverage);
            _plugin.AttachDelegate($"Player_{nameof(Incidents)}", () => Incidents);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureFrontLeft)}", () => TirePressureFrontLeft);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureFrontLeftAverage)}", () => TirePressureFrontLeftAverage);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureFrontRight)}", () => TirePressureFrontRight);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureFrontRightAverage)}", () => TirePressureFrontRightAverage);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureRearLeft)}", () => TirePressureRearLeft);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureRearLeftAverage)}", () => TirePressureRearLeftAverage);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureRearRight)}", () => TirePressureRearRight);
            _plugin.AttachDelegate($"Player_{nameof(TirePressureRearRightAverage)}", () => TirePressureRearRightAverage);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureFrontLeft)}", () => TireTemperatureFrontLeft);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureFrontLeftAverage)}", () => TireTemperatureFrontLeftAverage);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureFrontRight)}", () => TireTemperatureFrontRight);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureFrontRightAverage)}", () => TireTemperatureFrontRightAverage);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureRearLeft)}", () => TireTemperatureRearLeft);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureRearLeftAverage)}", () => TireTemperatureRearLeftAverage);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureRearRight)}", () => TireTemperatureRearRight);
            _plugin.AttachDelegate($"Player_{nameof(TireTemperatureRearRightAverage)}", () => TireTemperatureRearRightAverage);
        }

        private void GetBrakeTemperatures()
        {
            _brakeTemperatures.AddValues(StatusDatabase.BrakeTemperatureFrontLeft, StatusDatabase.BrakeTemperatureFrontRight, StatusDatabase.BrakeTemperatureRearLeft, StatusDatabase.BrakeTemperatureRearRight, 80);
        }

        private void GetIncidents()
        {
            if (StatusDatabase.GetRawDataObject() is DataSampleEx iRacingData)
            {
                iRacingData.Telemetry.TryGetValue("PlayerCarTeamIncidentCount", out object rawIncidents);

                Incidents = Convert.ToInt32(rawIncidents);
            }
        }

        private void GetTirePressures()
        {
            _tirePressures.AddValues(StatusDatabase.TyrePressureFrontLeft, StatusDatabase.TyrePressureFrontRight, StatusDatabase.TyrePressureRearLeft, StatusDatabase.TyrePressureRearRight, 80);
        }

        private void GetTireTemperatures()
        {
            _tireTemperatures.AddValues(StatusDatabase.TyreTemperatureFrontLeft, StatusDatabase.TyreTemperatureFrontRight, StatusDatabase.TyreTemperatureRearLeft, StatusDatabase.TyreTemperatureRearRight, 80);
        }

        private void OnPluginDataUpdated(object sender, NotifyDataUpdatedEventArgs e)
        {
            _counter++;

            if (OnTrack(e.Data) == true)
            {
                StatusDatabase = e.Data.NewData;

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
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureFrontLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureFrontLeftAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureFrontRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureFrontRightAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureRearLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureRearLeftAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureRearRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(BrakeTemperatureRearRightAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(Incidents)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureFrontLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureFrontLeftAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureFrontRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureFrontRightAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureRearLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureRearLeftAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureRearRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(TirePressureRearRightAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureFrontLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureFrontLeftAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureFrontRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureFrontRightAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureRearLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureRearLeftAverage)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureRearRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(TireTemperatureRearRightAverage)}");
        }

        private class CornerTelemetry
        {
            private readonly Queue<(double FrontLeft, double FrontRight, double RearLeft, double RearRight)> _values = new ();

            public double FrontLeft => _values.LastOrDefault().FrontLeft;

            public double FrontLeftAverage { get; private set; }

            public double FrontRight => _values.LastOrDefault().FrontRight;

            public double FrontRightAverage { get; private set; }

            public double RearLeft => _values.LastOrDefault().RearLeft;

            public double RearLeftAverage { get; private set; }

            public double RearRight => _values.LastOrDefault().RearRight;

            public double RearRightAverage { get; private set; }

            public void AddValues(double frontLeft, double frontRight, double rearLeft, double rearRight, int maxSamples)
            {
                _values.Enqueue((frontLeft, frontRight, rearLeft, rearRight));

                while (_values.Count > maxSamples)
                {
                    _values.Dequeue();
                }

                FrontLeftAverage = _values.Average(x => x.FrontLeft);
                FrontRightAverage = _values.Average(x => x.FrontRight);
                RearLeftAverage = _values.Average(x => x.RearLeft);
                RearRightAverage = _values.Average(x => x.RearRight);
            }
        }
    }
}