using System;
using System.Linq;

namespace PostItNoteRacing.Plugin
{
    internal class Driver
    {
        private static readonly char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public double? IRating { get; set; }

        public int IRatingChange { get; set; }

        public string IRatingString => $"{(IRating ?? 0D) / 1000:0.0k}";

        public string IRatingLicenseCombinedString => $"{License.ShortString} {IRatingString}";

        public bool IsActive { get; set; }

        public int LapsCompleted { get; set; }

        public License License { get; set; }

        public string Name { get; set; }

        public string ShortName
        {
            get
            {
                if (Name != null)
                {
                    return $"{Name.Split(' ')[0].Substring(0, 1)}. {String.Join(" ", Name.Split(' ').Skip(1)).TrimEnd(digits)}";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
