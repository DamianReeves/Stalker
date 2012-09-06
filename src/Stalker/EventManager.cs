using System;

namespace Stalker {
    public static class EventManager {
        //This overload handles any type of EventHandler
        public static IDisposable Subscribe<TSubscriber, TDelegate, TEventArgs>(
            Func<EventHandler<TEventArgs>, TDelegate> converter,
            Action<TDelegate> add, Action<TDelegate> remove,
            TSubscriber subscriber, Action<TSubscriber, TEventArgs> action)
            where TEventArgs : EventArgs
            where TDelegate : class
            where TSubscriber : class {
            var subsWeakRef = new WeakReference(subscriber);
            TDelegate[] handler = {null};
            handler[0] = converter(new EventHandler<TEventArgs>(
                (s, e) => {
                    var subsStrongRef = subsWeakRef.Target as TSubscriber;
                    if (subsStrongRef != null) {
                        action(subsStrongRef, e);
                    } else {
                        remove(handler[0]);
                        handler[0] = null;
                    }
                }));
            add(handler[0]);
            return new AnonymousDisposable(()=>remove(handler[0]));
        }

        // this overload is simplified for generic EventHandlers
        public static IDisposable Subscribe<TSubscriber, TArgs>(
            Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove,
            TSubscriber subscriber, Action<TSubscriber, TArgs> action)
            where TArgs : EventArgs
            where TSubscriber : class {
            return Subscribe<TSubscriber, EventHandler<TArgs>, TArgs>(
                h => h, add, remove, subscriber, action);
        }
    }
}