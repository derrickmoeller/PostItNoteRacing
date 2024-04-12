namespace PostItNoteRacing.Plugin.Models
{
    internal class License
    {
        private const string Blue = "#FF0153DB";
        private const string Green = "#FF00C702";
        private const string Orange = "#FFFC8A27";
        private const string Red = "#FFB40800";
        private const string Yellow = "#FFFEEC04";

        public string Color
        {
            get
            {
                switch (ShortString)
                {
                    case "A":
                        return Blue;
                    case "B":
                        return Green;
                    case "C":
                        return Yellow;
                    case "D":
                        return Orange;
                    case "R":
                        return Red;
                    default:
                        return Colors.White;
                }
            }
        }

        public string ShortString => (String ?? string.Empty).Split(' ')[0];

        public string String { get; set; }

        public string TextColor
        {
            get
            {
                switch (Color)
                {
                    case Blue:
                    case Red:
                        return Colors.White;
                    default:
                        return Colors.Black;
                }
            }
        }
    }
}
