using System;

namespace PostItNoteRacing.Plugin
{
    internal class License
    {
        private const string blue = "#FF0153DB";
        private const string green = "#FF00C702";
        private const string orange = "#FFFC8A27";
        private const string red = "#FFB40800";
        private const string yellow = "#FFFEEC04";

        public string Color
        {
            get
            {
                switch (ShortString)
                {
                    case "A":
                        return blue;
                    case "B":
                        return green;
                    case "C":
                        return yellow;
                    case "D":
                        return orange;
                    case "R":
                        return red;
                    default:
                        return Colors.White;
                }
            }
        }

        public string ShortString => (String ?? String.Empty).Split(' ')[0];

        public string String { get; set; }

        public string TextColor
        {
            get
            {
                switch (Color)
                {
                    case blue:
                    case red:
                        return Colors.White;
                    default:
                        return Colors.Black;
                }
            }
        }
    }
}
