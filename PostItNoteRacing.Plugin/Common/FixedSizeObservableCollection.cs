using System.Collections.ObjectModel;

namespace PostItNoteRacing.Common
{
    public class FixedSizeObservableCollection<T>(int maxSize) : ObservableCollection<T>
    {
        private readonly int _maxSize = maxSize;

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            while (Count > _maxSize)
            {
                RemoveAt(0);
            }
        }
    }
}
