namespace Stalker {
    using System.Runtime.CompilerServices;

    public class Stalker: IStalker {
        readonly ConditionalWeakTable<object, TrackedItem> _trackedTargets = new ConditionalWeakTable<object, TrackedItem>(); 


        public void Watch(object target) {
            _trackedTargets.GetOrCreateValue(target);
            
        }

        public IWatcher AddWatcher(IWatcher watcher) {
            throw new System.NotImplementedException();
        }
    }

    public interface IStalker {
        void Watch(object target);
        IWatcher AddWatcher(IWatcher watcher);
    }
}
