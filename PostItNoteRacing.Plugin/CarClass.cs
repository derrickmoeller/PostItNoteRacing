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

        public int Index
        {
            get
            {
                return Colors.IndexOf(Color) + 1;
            }
        }

        public string Name { get; set; }

        public string TextColor { get; set; }
    }
}
