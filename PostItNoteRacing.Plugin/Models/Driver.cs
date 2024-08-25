using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using System;
using System.Globalization;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Driver : Entity, INotifyBestLapChanged
    {
        private static readonly char[] Digits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
        private static readonly TextInfo TextInfo = new CultureInfo("en-US").TextInfo;

        private readonly INotifyBestLapChanged _carClass;

        private Lap _bestLap;

        public Driver(IModifySimHub plugin, int index, INotifyBestLapChanged carClass)
            : base(plugin, index)
        {
            _carClass = carClass;
            _carClass.BestLapChanged += OnCarClassBestLapChanged;
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

        public string IRatingLicenseCombinedString => $"{License.ShortString} {IRatingString}";

        public string IRatingString => $"{(IRating ?? 0D) / 1000:0.0k}";

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

        protected override void AttachDelegates()
        {
            Plugin.AttachDelegate($"Driver_{Index:D2}_BestLapColor", () => BestLapColor);
            Plugin.AttachDelegate($"Driver_{Index:D2}_BestLapTime", () => BestLap?.Time ?? TimeSpan.Zero);
            Plugin.AttachDelegate($"Driver_{Index:D2}_IRating", () => IRating);
            Plugin.AttachDelegate($"Driver_{Index:D2}_IRatingChange", () => IRatingChange);
            Plugin.AttachDelegate($"Driver_{Index:D2}_IRatingLicenseCombinedString", () => IRatingLicenseCombinedString);
            Plugin.AttachDelegate($"Driver_{Index:D2}_IRatingString", () => IRatingString);
            Plugin.AttachDelegate($"Driver_{Index:D2}_LapsCompleted", () => LapsCompleted);
            Plugin.AttachDelegate($"Driver_{Index:D2}_LicenseColor", () => License.Color);
            Plugin.AttachDelegate($"Driver_{Index:D2}_LicenseShortString", () => License.ShortString);
            Plugin.AttachDelegate($"Driver_{Index:D2}_LicenseString", () => License.String);
            Plugin.AttachDelegate($"Driver_{Index:D2}_LicenseTextColor", () => License.TextColor);
            Plugin.AttachDelegate($"Driver_{Index:D2}_Name", () => Name);
            Plugin.AttachDelegate($"Driver_{Index:D2}_ShortName", () => ShortName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_carClass != null)
                {
                    _carClass.BestLapChanged -= OnCarClassBestLapChanged;
                }
            }

            base.Dispose(disposing);
        }

        protected override void TryDetachDelegates()
        {
            Plugin?.DetachDelegate($"Driver_{Index:D2}_BestLapColor");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_BestLapTime");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_IRating");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_IRatingChange");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_IRatingLicenseCombinedString");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_IRatingString");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_LapsCompleted");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_LicenseColor");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_LicenseShortString");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_LicenseString");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_LicenseTextColor");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_Name");
            Plugin?.DetachDelegate($"Driver_{Index:D2}_ShortName");
        }

        private void OnBestLapChanged()
        {
            BestLapChanged?.Invoke(this, new BestLapChangedEventArgs(BestLap));
        }

        private void OnCarClassBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (BestLap?.Time > TimeSpan.Zero && BestLap.Time == e.Lap?.Time)
            {
                BestLapColor = Colors.Purple;
            }
            else
            {
                BestLapColor = Colors.White;
            }
        }

        #region Interface: INotifyBestLapChanged
        public event EventHandler<BestLapChangedEventArgs> BestLapChanged;
        #endregion
    }
}
