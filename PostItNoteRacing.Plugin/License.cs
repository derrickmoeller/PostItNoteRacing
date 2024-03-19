using System;

namespace PostItNoteRacing.Plugin
{
    internal class License
    {
        public string Color
        {
            get
            {
                switch ((String ?? String.Empty).Split(' ')[0])
                {
                    case "A":
                        return "#0153DB";
                    case "B":
                        return "#00C702";
                    case "C":
                        return "#FEEC04";
                    case "D":
                        return "#FC8A27";
                    case "R":
                        return "#B40800";
                    default:
                        return "#FFFFFF";
                }
            }
        }

        public string String { get; set; }

        public string TextColor
        {
            get
            {
                switch (Color)
                {
                    case "#0153DB":
                    case "#B40800":
                        return "White";
                    default:
                        return "Black";
                }
            }
        }
    }
}
