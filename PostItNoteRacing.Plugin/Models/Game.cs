using System.Collections.Generic;

namespace PostItNoteRacing.Plugin.Models
{
    internal class Game(string name)
    {
        private readonly List<string> _supportedGames = new () { "IRacing" };
        private readonly List<string> _unsupportedGames = new () { "AssettoCorsa", "Automobilista2", "RFactor2" };

        public bool IsIRacing => Name == "IRacing";

        public bool? IsSupported
        {
            get
            {
                if (_unsupportedGames.Contains(Name))
                {
                    return false;
                }
                else if (_supportedGames.Contains(Name))
                {
                    return true;
                }
                else
                {
                    return null;
                }
            }
        }

        public string Name { get; private set; } = name;
    }
}