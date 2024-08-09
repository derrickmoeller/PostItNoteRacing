using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class CarClass(IModifySimHub plugin) : INotifyBestLapChanged
    {
        private readonly IModifySimHub _plugin = plugin;

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

        public int Index { get; set; }

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
                }

                for (int i = Teams.Count + 1; i <= 63; i++)
                {
                    _plugin.SetProperty($"Class_{Index:D2}_{i:D2}_LeaderboardPosition", -1);
                }
            }

            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (Team team in e.NewItems)
                {
                    team.BestLapChanged += OnTeamBestLapChanged;
                }
            }

            _plugin.SetProperty($"Class_{Index:D2}_OpponentCount", Teams.Count);
        }

        #region Interface: INotifyBestLapChanged
        public event EventHandler<BestLapChangedEventArgs> BestLapChanged;
        #endregion
    }
}
