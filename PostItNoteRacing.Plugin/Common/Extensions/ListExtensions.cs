using System.Collections;

namespace PostItNoteRacing.Common.Extensions
{
    public static class ListExtensions
    {
        public static void RemoveAll(this IList list)
        {
            while (list.Count > 0)
            {
                list.RemoveAt(0);
            }
        }
    }
}
