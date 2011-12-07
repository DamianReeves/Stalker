using System;

namespace Stalker {
    public class ChangeNotification<TSender, TValue> : IChangeNotification<TSender, TValue> {
        public TSender Sender { get; set; }
        public TValue Value { get; set; }
        public string PropertyName { get; set; }
    }

    public class ChangeNotification<TValue> : ChangeNotification<object,TValue> {}

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

    public interface IValueChangedNotification<out TSender, out TValue> : IChangeNotification<TSender, TValue> {
        TValue OldValue { get; }
    }

    public class ValueChangedNotification<TSender, TValue> : ChangeNotification<TSender, TValue>, IValueChangedNotification<TSender, TValue> {
        public TValue OldValue { get; set; }
    }
}