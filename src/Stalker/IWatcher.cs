using System;
using System.ComponentModel;
using System.Windows;

namespace Stalker {
    public interface IWatcher {
        void StartWatching(object target);
        void StopWatching(object target);
    }

    public class PropertyChangedWatcher : IWatcher {
        public void StartWatching(object target) {
            var npc = target as INotifyPropertyChanged;
            if (npc != null) {
                StartWatching(npc);
            }
        }

        public void StopWatching(object target) {

        }

        private void StartWatching(INotifyPropertyChanged npc) {
            if (npc == null) throw new ArgumentNullException("npc");
            PropertyChangedEventManager.AddListener(npc,
                new WeakEventListener((t, o, arg) => Console.WriteLine("PropertyChanged event: Type: {0}, Sender: {1}, EventArgs: {2}",t,o,arg)),
                string.Empty);
        }
    }

    public class WeakEventListener : IWeakEventListener {
        private readonly Func<Type, object, EventArgs, bool> _onEvent;

        public WeakEventListener(Func<Type, object, EventArgs, bool> onEvent) {
            if (onEvent == null) throw new ArgumentNullException("onEvent");
            _onEvent = onEvent;
        }

        public WeakEventListener(Action<Type, object, EventArgs> onEvent) {
            if (onEvent == null) throw new ArgumentNullException("onEvent");
            _onEvent = (type, sender, args) => {
                onEvent(type, sender, args);
                return true;
            };
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e) {
            return _onEvent(managerType, sender, e);
        }
    }
}