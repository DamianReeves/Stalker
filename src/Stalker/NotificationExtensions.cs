// -----------------------------------------------------------------------
// <copyright file="NotificationExtensions.cs" company="Damian Reeves">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
namespace Stalker {
    public static class NotificationExtensions {
        public static IObservable<EventPattern<PropertyChangedEventArgs>> ToObservable(this INotifyPropertyChanged notifier) {
            var propertyChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => (sender, args) => handler(sender, args),
                x => notifier.PropertyChanged += x,
                x => notifier.PropertyChanged -= x);
            return propertyChanged;
        }

        public static IObservable<TProperty> ObservePropertyChanged<TNotifier, TProperty>(this TNotifier notifier,
                                                                                          Expression<Func<TNotifier, TProperty>> propertyAccessor,
                                                                                          bool startWithCurrent = false)
            where TNotifier : INotifyPropertyChanged {

            // Parse the expression to find the correct property name.
            MemberExpression member = (MemberExpression)propertyAccessor.Body;
            string name = member.Member.Name;

            // Compile the expression so we can run it to read the property value.
            var reader = propertyAccessor.Compile();

            var propertyChanged = notifier.ToObservable();

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
            bool startWithCurrent = false) where TNotifier : INotifyPropertyChanged {

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

        public static IObservable<IChangeNotification<object, TProperty>> ObserveChanges<TNotifier, TProperty>(TNotifier notifier
            , Expression<Func<TNotifier, TProperty>> propertyAccessor
            , bool startWithCurrent = false) where TNotifier : INotifyPropertyChanged {

            var propertyName = propertyAccessor.GetMemberInfo().Name;
            // Compile the expression so we can run it to read the property value.
            var reader = propertyAccessor.Compile();

            var eventStream =
                from evt in notifier.ToObservable()
                where evt.EventArgs.PropertyName == propertyName
                select new ChangeNotification<object,TProperty>{
                    PropertyName = propertyName, 
                    Sender = evt.Sender,
                    Value = reader(notifier)
                };

            if (startWithCurrent) {
                var values = eventStream;
                var initial = new ChangeNotification<TProperty> {
                    Sender = notifier,
                    PropertyName = propertyName,
                    Value = reader(notifier)
                };
                eventStream = Observable.Defer(() => Observable.Return(initial)).Concat(values);
            }
            return eventStream.Scan(default(IChangeNotification<object,TProperty>)
                , (acc, notification) =>(IChangeNotification<object,TProperty>) 
                    new ValueChangedNotification<object, TProperty>{
                        PropertyName = notification.PropertyName,
                        Sender = notification.Sender,
                        Value = notification.Value,
                        OldValue = acc == null? default(TProperty) :acc.Value
                    });

        }
    }    
}
