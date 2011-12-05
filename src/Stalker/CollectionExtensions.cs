using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Stalker {
    public static class CollectionExtensions {
        public static IObservable<NotifyCollectionChangedEventArgs> ObserveCollectionChanged(this IEnumerable source) {
            var ncc = source as INotifyCollectionChanged;
            if (ncc != null) {
                return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => ncc.CollectionChanged += h,
                    h => ncc.CollectionChanged -= h
                ).Select(ev =>ev.EventArgs);
            }
            return Observable.Never<NotifyCollectionChangedEventArgs>();
        }

        public static IObservable<object> ObserveCollectionItemChanged(this IEnumerable source) {
            throw new NotImplementedException();
        }
    }

    [Flags]
    public enum ChangeType {
        None = 0,
        ValueChanged = 1,
        IsReadOnly = 2,
        ItemAdded = 4,
        ItemRemoved = 8,
        ItemReplaced = ItemRemoved | ItemAdded,
        CollectionReset = 16,
    }

    public interface IChangeNotification<out TSender, out TValue> {
        TSender Sender { get; }
        TValue Value { get; }
        string PropertyName { get; }
    }

    public class ChangeNotification<TSender,TValue>: IChangeNotification<TSender,TValue> {
        public TSender Sender { get; set; }
        public TValue Value { get; set; }
        public string PropertyName { get; set; }
    }
}
