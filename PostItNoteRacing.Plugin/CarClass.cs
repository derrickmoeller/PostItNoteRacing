using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PostItNoteRacing.Plugin
{
    internal class CarClass
    {
        private const string LightLimeGreen = "#53FF77";
        private const string LightPink = "#FF5888";
        private const string LightYellow = "#FFDA59";
        private const string VeryLightViolet = "#AE6BFF";
        private const string VividCyan = "#33CEFF";

        private string _name;
        
        public static ReadOnlyCollection<string> Colors =
            new ReadOnlyCollection<string>(new[]
            {
                LightYellow,
                VividCyan,
                LightPink,
                VeryLightViolet,
                LightLimeGreen
            });

        public string Color { get; set; }

        public int? Index
        {
            get
            {
                var index = Colors.IndexOf(Color);
                if (index == -1)
                {
                    return null;
                }
                else
                {
                    return index + 1;
                }
            }
        }

        public string Name
        {
            get { return _name; }
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

        public List<Team> Teams { get; set; }

        public string TextColor { get; set; }
    }
}
