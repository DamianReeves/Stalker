using System.ComponentModel;
using AddressBook;
using FluentAssertions;
using NUnit.Framework;

namespace Stalker.Prey {
    [TestFixture]
    public class NotificationTest {
        [Test]
        public void NotifyPropertyChanged_Amendment_IsApplied() {
            var person = new Person();
            var notifier = person as INotifyPropertyChanged;
            notifier.Should().NotBeNull();
        }
    }
}
