﻿using PostItNoteRacing.Plugin.EventArgs;
using System;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Driver
    {
        private static readonly char[] Digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private Lap _bestLap;

        public Driver(bool isPlayer)
        {
            if (isPlayer == true)
            {
                BestLapColor = Colors.Yellow;
            }
            else
            {
                BestLapColor = Colors.White;
            }
        }

        public event EventHandler<LapChangedEventArgs> BestLapChanged;

        public Lap BestLap
        {
            get => _bestLap;
            set
            {
                if (_bestLap != value)
                {
                    _bestLap = value;
                    OnBestLapChanged();
                }
            }
        }

        public string BestLapColor { get; set; }

        public double? IRating { get; set; }

        public int IRatingChange { get; set; }

        public string IRatingString => $"{(IRating ?? 0D) / 1000:0.0k}";

        public string IRatingLicenseCombinedString => $"{License.ShortString} {IRatingString}";

        public bool IsActive { get; set; }

        public int LapsCompleted { get; set; }

        public License License { get; set; }

        public string Name { get; set; }

        public string ShortName
        {
            get
            {
                if (Name != null)
                {
                    return $"{Name.Split(' ')[0].Substring(0, 1)}. {string.Join(" ", Name.Split(' ').Skip(1)).TrimEnd(Digits)}";
                }
                else
                {
                    return null;
                }
            }
        }

        private void OnBestLapChanged()
        {
            BestLapChanged?.Invoke(this, new LapChangedEventArgs(BestLap?.Time));
        }
    }
}
