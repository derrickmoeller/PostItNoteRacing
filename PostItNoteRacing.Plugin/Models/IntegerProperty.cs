namespace PostItNoteRacing.Plugin.Models
{
    internal class IntegerProperty
    {
        public IntegerProperty(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public int Maximum { get; set; } = 10;

        public int Minimum { get; set; } = 1;
    }
}