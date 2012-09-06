using System;
using FluentAssertions;
using NUnit.Framework;

namespace Stalker {
    [TestFixture]
    public class StalkerTest {
        [Test]
        public void WatchedObjectsCanBeGarbageCollected() {
            var watchedItem = new WatchedItem();
            var weakReference = new WeakReference(watchedItem);
            var stalker = new Stalker();
            stalker.Watch(watchedItem);
            watchedItem = null;
            GC.Collect();
            weakReference.ShouldHave().Properties(x=>x.IsAlive).EqualTo(new {IsAlive=false});
        }
    }

    internal class WatchedItem: NotifyingObject {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
