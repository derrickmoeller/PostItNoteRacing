using System.Collections.ObjectModel;

namespace PostItNoteRacing.Plugin
{
    internal class CarClass
    {
        private static readonly ReadOnlyCollection<string> _colors =
            new ReadOnlyCollection<string>(new []
            {
                "#FFDA59", //1: light yellow
                "#33CEFF", //2: vivid cyan
                "#FF5888", //3: light pink
                "#AE6BFF", //4: very light violet
                "#53FF77"  //5: light lime green
            });

        public static ReadOnlyCollection<string> Colors => _colors;
        
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
