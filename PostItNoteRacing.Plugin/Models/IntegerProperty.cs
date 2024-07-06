namespace PostItNoteRacing.Plugin.Models
{
    internal class IntegerProperty(int id)
    {
        public int Id { get; } = id;

        public int Maximum { get; set; } = 10;

        public int Minimum { get; set; } = 1;
    }
}