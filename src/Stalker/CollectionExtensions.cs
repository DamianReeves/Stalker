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
        public static IObservable<TProperty> ObservePropertyChanged<TNotifier, TProperty>(this TNotifier notifier,
            Expression<Func<TNotifier, TProperty>> propertyAccessor,
            bool startWithCurrent = false)
            where TNotifier : INotifyPropertyChanged {

            // Parse the expression to find the correct property name.
            MemberExpression member = (MemberExpression)propertyAccessor.Body;
            string name = member.Member.Name;

            // Compile the expression so we can run it to read the property value.
            var reader = propertyAccessor.Compile();

            var propertyChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => (sender, args) => handler(sender, args),
                x => notifier.PropertyChanged += x,
                x => notifier.PropertyChanged -= x);

            // Filter the events to the correct property name, then select the value of the property from the notifier.
            var newValues = from p in propertyChanged
                            where p.EventArgs.PropertyName == name
                            select reader(notifier);

            // If the caller wants the current value as well as future ones, use Defer() so that the current value is read when the subscription
            // is added, rather than right now. Otherwise just return the above observable.
            return startWithCurrent ? Observable.Defer(() => Observable.Return(reader(notifier)).Concat(newValues)) : newValues;
        }

        public static IObservable<Tuple<TProperty, TProperty>> ObserveValueChanged<TNotifier, TProperty>(this TNotifier notifier,
            Expression<Func<TNotifier, TProperty>> propertyAccessor,
            bool startWithCurrent = false)
            where TNotifier : INotifyPropertyChanged {

            // Compile the expression so we can run it to read the property value.
            var reader = propertyAccessor.Compile();

            var observable = ObservePropertyChanged(notifier, propertyAccessor);
            if (startWithCurrent) {
                var captured = observable;
                observable = Observable.Defer(
                    () => Observable.Return(reader(notifier)).Concat(captured));
            }
            return observable.Scan(default(Tuple<TProperty, TProperty>)
                , (acc, p) => Tuple.Create(acc == null ? default(TProperty) : acc.Item2, p));

        }

        public static IObservable<Tuple<TProperty, TProperty>> ObserveValueChanged2<TNotifier, TProperty>(this TNotifier notifier,
            Expression<Func<TNotifier, TProperty>> propertyAccessor,
            bool startWithCurrent = false)
            where TNotifier : INotifyPropertyChanged {

            // Compile the expression so we can run it to read the property value.
            var reader = propertyAccessor.Compile();

            var newValues = ObservePropertyChanged(notifier, propertyAccessor, false);
            if (startWithCurrent) {
                var capturedNewValues = newValues; //To prevent warning about modified closure
                newValues = Observable.Defer(() => Observable.Return(reader(notifier))
                                        .Concat(capturedNewValues));
            }

            return Observable.Create<Tuple<TProperty, TProperty>>(obs => {
                Tuple<TProperty, TProperty> oldNew = null;
                return newValues.Subscribe(v => {
                    if (oldNew == null) {
                        oldNew = Tuple.Create(default(TProperty), v);
                    } else {
                        oldNew = Tuple.Create(oldNew.Item2, v);
                        obs.OnNext(oldNew);
                    }
                },
                    obs.OnError,
                    obs.OnCompleted);
            });
        }

        public static MemberInfo GetMemberInfo(this Expression expression) {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression) {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            } else memberExpression = (MemberExpression)lambda.Body;

            return memberExpression.Member;
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
