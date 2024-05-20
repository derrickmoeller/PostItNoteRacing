using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using System;
using System.Globalization;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Driver : IDisposable, INotifyBestLapChanged
    {
        private static readonly char[] Digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static readonly TextInfo TextInfo = new CultureInfo("en-US").TextInfo;

        private readonly INotifyBestLapChanged _carClass;
        private readonly bool _isPlayer;

        private Lap _bestLap;

        public Driver(INotifyBestLapChanged carClass, bool isPlayer)
        {
            _carClass = carClass;
            _carClass.BestLapChanged += OnCarClassBestLapChanged;

            _isPlayer = isPlayer;

            if (_isPlayer == true)
            {
                BestLapColor = Colors.Yellow;
            }
            else
            {
                BestLapColor = Colors.White;
            }
        }

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

        public string BestLapColor { get; private set; }

        public double? IRating { get; set; }

        public int IRatingChange { get; set; }

        public string IRatingString => $"{(IRating ?? 0D) / 1000:0.0k}";

        public string IRatingLicenseCombinedString => $"{License.ShortString} {IRatingString}";

        public int LapsCompleted { get; set; }

        public License License { get; set; }

        public string Name { get; set; }

        public string ShortName
        {
            get
            {
                if (Name != null)
                {
                    return TextInfo.ToTitleCase($"{Name.Split(' ')[0].Substring(0, 1)}. {string.Join(" ", Name.Split(' ').Skip(1)).TrimEnd(Digits)}");
                }
                else
                {
                    return null;
                }
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_carClass != null)
                {
                    _carClass.BestLapChanged -= OnCarClassBestLapChanged;
                }
            }
        }

        private void OnBestLapChanged()
        {
            BestLapChanged?.Invoke(this, new BestLapChangedEventArgs(BestLap));
        }

        private void OnCarClassBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (BestLap?.Time > TimeSpan.Zero && BestLap.Time == e.Lap.Time)
            {
                BestLapColor = Colors.Purple;
            }
            else if (_isPlayer == true)
            {
                BestLapColor = Colors.Yellow;
            }
            else
            {
                BestLapColor = Colors.White;
            }
        }

        #region Interface: IDisposable
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Interface: INotifyBestLapChanged
        public event EventHandler<BestLapChangedEventArgs> BestLapChanged;
        #endregion
    }
}
