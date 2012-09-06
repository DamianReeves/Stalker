using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Stalker {
    public class EventManagerTest {
        [Test]
        public void SubscribedEventIsActedOn() {
            var sink = new EventSink();
            var mockListener = new Mock<IEventListener>();
            mockListener.Setup(l=>l.OnGenericEvent()).Verifiable();

            EventManager.SubscribeTo<IEventListener, EventArgs>(
                h => sink.GenericEvent += h,
                h => sink.GenericEvent -= h,
                mockListener.Object,
                (l,args)=>l.OnGenericEvent()
            );

            sink.OnGenericEvent(EventArgs.Empty);
            mockListener.Verify(l=>l.OnGenericEvent(), Times.Once());
        }

        [Test]
        public void DisposingOfSubscriptionUnregistersEvent() {
            var sink = new EventSink();
            var mockListener = new Mock<IEventListener>();
            mockListener.Setup(l => l.OnGenericEvent()).Verifiable();

            using(EventManager.SubscribeTo<IEventListener, EventArgs>(
                h => sink.GenericEvent += h,
                h => sink.GenericEvent -= h,
                mockListener.Object,
                (l, args) => l.OnGenericEvent()
            )) {
                sink.OnGenericEvent(EventArgs.Empty);
                sink.OnGenericEvent(EventArgs.Empty);
            }

            sink.OnGenericEvent(EventArgs.Empty);
            mockListener.Verify(l => l.OnGenericEvent(), Times.Exactly(2));
        }

        [Test]
        public void SubscripingToEventDoesNotKeepItAlive() {
            var sink = new EventSink();
            var mockListener = new Mock<IEventListener>();
            mockListener.Setup(l => l.OnGenericEvent()).Verifiable();

            EventManager.SubscribeTo<IEventListener, EventArgs>(
                h => sink.GenericEvent += h,
                h => sink.GenericEvent -= h,
                mockListener.Object,
                (l, args) => l.OnGenericEvent()
            );

            var weakReference = new WeakReference(sink);
            sink = null;
            GC.Collect();
            weakReference.ShouldHave().Properties(x=>x.IsAlive).EqualTo(new{IsAlive=false});
        }

        public interface IEventListener {
            void OnGenericEvent();
            void OnSimpleEvent();
        }

        public class EventSink {
            public event EventHandler<EventArgs> GenericEvent;
            public event EventHandler SimpleEvent;

            public void OnSimpleEvent(EventArgs e) {
                EventHandler handler = SimpleEvent;
                if (handler != null) handler(this, e);
            }

            public void OnGenericEvent(EventArgs e) {
                EventHandler<EventArgs> handler = GenericEvent;
                if (handler != null) handler(this, e);
            }
        }
    }
}
