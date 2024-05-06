using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PostItNoteRacing.Plugin.Models
{
    internal class CarClass : INotifyBestLapChanged
    {
        private const string LightLimeGreen = "#53FF77";
        private const string LightPink = "#FF5888";
        private const string LightYellow = "#FFDA59";
        private const string VeryLightViolet = "#AE6BFF";
        private const string VividCyan = "#33CEFF";

        private TimeSpan? _bestLapTime;
        private string _name;
        private ObservableCollection<Team> _teams;

        public static ReadOnlyCollection<string> Colors { get; } =
            new ReadOnlyCollection<string>(new[]
            {
                LightYellow,
                VividCyan,
                LightPink,
                VeryLightViolet,
                LightLimeGreen,
            });

        public string Color { get; set; }

        public int Index
        {
            get
            {
                var index = Colors.IndexOf(Color);
                if (index == -1)
                {
                    return 1;
                }
                else
                {
                    return index + 1;
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                switch (value)
                {
                    case "Nissan GTP":
                        _name = "GTP";
                        break;
                    case "Dallara P217":
                    case "HPD ARX-01c":
                        _name = "LMP2";
                        break;
                    case "Ligier JS P320":
                        _name = "LMP3";
                        break;
                    case "GT1 Class":
                        _name = "GT1";
                        break;
                    case "Ford GT":
                        _name = "GT2";
                        break;
                    case "IMSA23":
                        _name = "GTD";
                        break;
                    case "GT4 Class":
                        _name = "GT4";
                        break;
                    case "Audi 90 GTO":
                        _name = "GTO";
                        break;
                    default:
                        _name = value;
                        break;
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

        private TimeSpan? BestLapTime
        {
            get => _bestLapTime;
            set
            {
                if (_bestLapTime != value)
                {
                    _bestLapTime = value;
                    OnBestLapTimeChanged();
                }
            }
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

        private void OnBestLapTimeChanged()
        {
            BestLapChanged?.Invoke(this, new BestLapChangedEventArgs(BestLapTime));
        }

        private void OnTeamBestLapChanged(object sender, BestLapChangedEventArgs e)
        {
            if (e.LapTime < (BestLapTime ?? TimeSpan.MaxValue))
            {
                BestLapTime = e.LapTime;
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
            }

            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (Team team in e.NewItems)
                {
                    team.BestLapChanged += OnTeamBestLapChanged;
                }
            }
        }

        #region Interface: INotifyBestLapChanged
        public event EventHandler<BestLapChangedEventArgs> BestLapChanged;
        #endregion
    }
}
