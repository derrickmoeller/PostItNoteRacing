namespace PostItNoteRacing.Plugin.Models
{
    internal class SimHubAction
    {
        public SimHubAction(string name, string friendlyName)
        {
            Name = name;
            FriendlyName = friendlyName;
        }

        public string FriendlyName { get; }

        public string Name { get; }
    }
}