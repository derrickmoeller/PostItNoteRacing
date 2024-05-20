using PostItNoteRacing.Common.Converters;
using System.ComponentModel;

namespace PostItNoteRacing.Plugin.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    internal enum ReferenceLap
    {
        [Description("Personal Best Lap")]
        PersonalBest,
        [Description("Team Best N Laps Average")]
        TeamBestN,
        [Description("Team Last Lap")]
        TeamLast,
        [Description("Team Last N Laps Average")]
        TeamLastN,
        [Description("Class Best Lap")]
        ClassBest,
    }
}