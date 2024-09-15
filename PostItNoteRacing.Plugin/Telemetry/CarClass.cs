using PostItNoteRacing.Common.Extensions;
using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PostItNoteRacing.Plugin.Telemetry
{
    internal class CarClass(IModifySimHub plugin, int index, object livePositionLock) : Entity(plugin, index), INotifyBestLapChanged
    {
        private readonly object _livePositionLock = livePositionLock;

        private Lap _bestLap;
        private ObservableCollection<Team> _teams;

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

        public string Color { get; set; }

        public string Name { get; set; }

        public string ShortName
        {
            get
            {
                switch (Name)
                {
                    case "Nissan GTP":
                        return "GTP";
                    case "Dallara P217":
                    case "HPD ARX-01c":
                        return "LMP2";
                    case "Ligier JS P320":
                        return "LMP3";
                    case "GT1 Class":
                        return "GT1";
                    case "Ford GT":
                        return "GT2";
                    case "IMSA23":
                        return "GTD";
                    case "GT4 Class":
                        return "GT4";
                    case "Audi 90 GTO":
                        return "GTO";
                    default:
                        return Name;
                }
            }
        }

        public int StrengthOfField => GetStrengthOfField(Teams.Where(x => x.IRating > 0).Select(x => x.IRating.Value));

        public string StrengthOfFieldString => $"{StrengthOfField / 1000D:0.0k}";

        public ObservableCollection<Team> Teams
        {
            get
            {
                if (_teams == null)
                {
                    _teams = new ObservableCollection<Team>();
                    _teams.CollectionChanged += OnTeamsCollectionChanged;
                }

                return _teams;
            }
        }

        public string TextColor { get; set; }

        protected override void AttachDelegates()
        {
            Plugin.AttachDelegate($"Class_{Index:D2}_Color", () => Color);
            Plugin.AttachDelegate($"Class_{Index:D2}_Index", () => Index);
            Plugin.AttachDelegate($"Class_{Index:D2}_Name", () => ShortName);
            Plugin.AttachDelegate($"Class_{Index:D2}_OpponentCount", () => Teams.Count);
            Plugin.AttachDelegate($"Class_{Index:D2}_SoF", () => StrengthOfField);
            Plugin.AttachDelegate($"Class_{Index:D2}_SoFString", () => StrengthOfFieldString);
            Plugin.AttachDelegate($"Class_{Index:D2}_TextColor", () => TextColor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_teams != null)
                {
                    _teams.RemoveAll();
                    _teams.CollectionChanged -= OnTeamsCollectionChanged;
                }
            }

            base.Dispose(disposing);
        }

        protected override void TryDetachDelegates()
        {
            Plugin?.DetachDelegate($"Class_{Index:D2}_Color");
            Plugin?.DetachDelegate($"Class_{Index:D2}_Index");
            Plugin?.DetachDelegate($"Class_{Index:D2}_Name");
            Plugin?.DetachDelegate($"Class_{Index:D2}_OpponentCount");
            Plugin?.DetachDelegate($"Class_{Index:D2}_SoF");
            Plugin?.DetachDelegate($"Class_{Index:D2}_SoFString");
            Plugin?.DetachDelegate($"Class_{Index:D2}_TextColor");
        }

        private static int GetStrengthOfField(IEnumerable<int> iRatings)
        {
            double sum = 0;
            double weight = 1600 / Math.Log(2);

            foreach (var iRating in iRatings)
            {
                sum += Math.Pow(2, -iRating / 1600D);
            }

            return (int)Math.Round(weight * Math.Log(iRatings.Count() / sum));
        }

        private void OnBestLapChanged()
        {
            BestLapChanged?.Invoke(this, new BestLapChangedEventArgs(BestLap));
        }

        private void OnTeamBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (e.Lap == null || e.Lap.Time < (BestLap?.Time ?? TimeSpan.MaxValue))
            {
                BestLap = e.Lap;
            }
        }

        private void OnTeamsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (Team team in e.OldItems)
                {
                    team.BestLapChanged -= OnTeamBestLapChanged;

                    Plugin.DetachDelegate($"Team_{team.Index:D2}_Class");
                }

                for (int i = Teams.Count + 1; i <= Teams.Count + e.OldItems.Count; i++)
                {
                    Plugin.DetachDelegate($"Class_{Index:D2}_LivePosition_{i:D2}_Team");
                }
            }

            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (Team team in e.NewItems)
                {
                    team.BestLapChanged += OnTeamBestLapChanged;

                    Plugin.AttachDelegate($"Team_{team.Index:D2}_Class", () => Index);
                }

                for (int i = Teams.Count - e.NewItems.Count + 1; i <= Teams.Count; i++)
                {
                    int j = i;

                    Plugin.AttachDelegate($"Class_{Index:D2}_LivePosition_{i:D2}_Team", () =>
                    {
                        lock (_livePositionLock)
                        {
                            return Teams.SingleOrDefault(x => x.LivePositionInClass == j)?.Index;
                        }
                    });
                }
            }
        }

        #region Interface: INotifyBestLapChanged
        public event EventHandler<BestLapChangedEventArgs> BestLapChanged;
        #endregion
    }
}
