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
        private readonly Queue<(double trackPosition, double frontLeft, double frontRight, double rearLeft, double rearRight)> _brakeTemperatures = new ();
        private readonly Queue<(double trackPosition, double frontLeft, double frontRight, double rearLeft, double rearRight)> _tirePressures = new ();
        private readonly Queue<(double trackPosition, double frontLeft, double frontRight, double rearLeft, double rearRight)> _tireTemperatures = new ();
        private readonly IModifySimHub _plugin;

        private int _counter;

        public Player(IModifySimHub plugin)
        {
            _plugin = plugin;
            _plugin.DataUpdated += OnPluginDataUpdated;

            AttachDelegates();
        }

        private double AvgBrakeTemperatureFrontLeft => _brakeTemperatures.Average(x => x.frontLeft);

        private double AvgBrakeTemperatureFrontRight => _brakeTemperatures.Average(x => x.frontRight);

        private double AvgBrakeTemperatureRearLeft => _brakeTemperatures.Average(x => x.rearLeft);

        private double AvgBrakeTemperatureRearRight => _brakeTemperatures.Average(x => x.rearRight);

        private double AvgTirePressureFrontLeft => _tirePressures.Average(x => x.frontLeft);

        private double AvgTirePressureFrontRight => _tirePressures.Average(x => x.frontRight);

        private double AvgTirePressureRearLeft => _tirePressures.Average(x => x.rearLeft);

        private double AvgTirePressureRearRight => _tirePressures.Average(x => x.rearRight);

        private double AvgTireTemperatureFrontLeft => _tireTemperatures.Average(x => x.frontLeft);

        private double AvgTireTemperatureFrontRight => _tireTemperatures.Average(x => x.frontRight);

        private double AvgTireTemperatureRearLeft => _tireTemperatures.Average(x => x.rearLeft);

        private double AvgTireTemperatureRearRight => _tireTemperatures.Average(x => x.rearRight);

        private int Incidents { get; set; }

        private StatusDataBase StatusDatabase { get; set; }

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
            _plugin.AttachDelegate($"Player_{nameof(AvgBrakeTemperatureFrontLeft)}", () => AvgBrakeTemperatureFrontLeft);
            _plugin.AttachDelegate($"Player_{nameof(AvgBrakeTemperatureFrontRight)}", () => AvgBrakeTemperatureFrontRight);
            _plugin.AttachDelegate($"Player_{nameof(AvgBrakeTemperatureRearLeft)}", () => AvgBrakeTemperatureRearLeft);
            _plugin.AttachDelegate($"Player_{nameof(AvgBrakeTemperatureRearRight)}", () => AvgBrakeTemperatureRearRight);
            _plugin.AttachDelegate($"Player_{nameof(AvgTirePressureFrontLeft)}", () => AvgTirePressureFrontLeft);
            _plugin.AttachDelegate($"Player_{nameof(AvgTirePressureFrontRight)}", () => AvgTirePressureFrontRight);
            _plugin.AttachDelegate($"Player_{nameof(AvgTirePressureRearLeft)}", () => AvgTirePressureRearLeft);
            _plugin.AttachDelegate($"Player_{nameof(AvgTirePressureRearRight)}", () => AvgTirePressureRearRight);
            _plugin.AttachDelegate($"Player_{nameof(AvgTireTemperatureFrontLeft)}", () => AvgTireTemperatureFrontLeft);
            _plugin.AttachDelegate($"Player_{nameof(AvgTireTemperatureFrontRight)}", () => AvgTireTemperatureFrontRight);
            _plugin.AttachDelegate($"Player_{nameof(AvgTireTemperatureRearLeft)}", () => AvgTireTemperatureRearLeft);
            _plugin.AttachDelegate($"Player_{nameof(AvgTireTemperatureRearRight)}", () => AvgTireTemperatureRearRight);
            _plugin.AttachDelegate($"Player_{nameof(Incidents)}", () => Incidents);
        }

        private void GetBrakeTemperatures()
        {
            _brakeTemperatures.Enqueue((StatusDatabase.TrackPositionPercent, StatusDatabase.BrakeTemperatureFrontLeft, StatusDatabase.BrakeTemperatureFrontRight, StatusDatabase.BrakeTemperatureRearLeft, StatusDatabase.BrakeTemperatureRearRight));
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
            _tirePressures.Enqueue((StatusDatabase.TrackPositionPercent, StatusDatabase.TyrePressureFrontLeft, StatusDatabase.TyrePressureFrontRight, StatusDatabase.TyrePressureRearLeft, StatusDatabase.TyrePressureRearRight));
        }

        private void GetTireTemperatures()
        {
            _tireTemperatures.Enqueue((StatusDatabase.TrackPositionPercent, StatusDatabase.TyreTemperatureFrontLeft, StatusDatabase.TyreTemperatureFrontRight, StatusDatabase.TyreTemperatureRearLeft, StatusDatabase.TyreTemperatureRearRight));
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
            _plugin?.DetachDelegate($"Player_{nameof(AvgBrakeTemperatureFrontLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgBrakeTemperatureFrontRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgBrakeTemperatureRearLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgBrakeTemperatureRearRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTirePressureFrontLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTirePressureFrontRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTirePressureRearLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTirePressureRearRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTireTemperatureFrontLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTireTemperatureFrontRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTireTemperatureRearLeft)}");
            _plugin?.DetachDelegate($"Player_{nameof(AvgTireTemperatureRearRight)}");
            _plugin?.DetachDelegate($"Player_{nameof(Incidents)}");
        }
    }
}
