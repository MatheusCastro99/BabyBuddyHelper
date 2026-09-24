using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BabyBuddyHelper.Core.Collections
{
    //ObservableCollection that can add many items with one notification. Adding N items one at a time raises N CollectionChanged
    //events, and every subscribed page rebuilds on each one; AddRange raises a single Reset instead.
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();

            int countBefore = Count;

            foreach (T item in items)
            {
                Items.Add(item); //Items bypasses the per-item notification
            }

            if (Count == countBefore)
                return;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
