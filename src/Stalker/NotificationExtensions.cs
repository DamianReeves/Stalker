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
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
namespace Stalker {
    
    
    public static class NotificationExtensions {
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
                                   , (acc, p) => Tuple.Create<TProperty, TProperty>(acc == null ? default(TProperty) : acc.Item2, p));

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
                                                                                                         oldNew = Tuple.Create<TProperty, TProperty>(default(TProperty), v);
                                                                                                     } else {
                                                                                                         oldNew = Tuple.Create<TProperty, TProperty>(oldNew.Item2, v);
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
}
