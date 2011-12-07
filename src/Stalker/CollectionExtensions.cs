using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;

namespace Stalker {
    public static partial class CollectionExtensions {
        /// <summary>
        /// Returns an observable sequence of the source collection change notifications.
        /// Returns Observable.Never for collections not implementing INCC.
        /// </summary>
        /// <param name="source">Collection to observe.</param>
        /// <returns>Observable sequence.</returns>
        public static IObservable<NotifyCollectionChangedEventArgs> ObserveCollectionChanged(
            this IEnumerable source) {
            var notifying = source as INotifyCollectionChanged;
            if (notifying != null) {
                return ObserveCollectionChanged(notifying);
            }

#if IncludeBindingList
            var bindingList = source as IBindingList;
            if (bindingList != null) {
                return ObserveCollectionChanged(bindingList);
            }
#endif
            return Observable.Never<NotifyCollectionChangedEventArgs>();
        }

        public static IObservable<NotifyCollectionChangedEventArgs> ObserveCollectionChanged(this INotifyCollectionChanged source) {
            if (source == null) return Observable.Never<NotifyCollectionChangedEventArgs>();
            return Observable.FromEventPattern<
                    NotifyCollectionChangedEventHandler,
                    NotifyCollectionChangedEventArgs>(
                        ev => source.CollectionChanged += ev,
                        ev => source.CollectionChanged -= ev)
                    .Select(x => x.EventArgs);
        }

#if IncludeBindingList            
            /// <summary>
            /// Wraps the <see cref="IBindingList"/> in 
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public static IObservable<NotifyCollectionChangedEventArgs> ObserveCollectionChanged(this IBindingList source) {
                if (source == null) return Observable.Never<NotifyCollectionChangedEventArgs>();
                var collectionView = new BindingListCollectionView(source);
                return ObserveCollectionChanged((INotifyCollectionChanged)collectionView);
            }
#endif
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
