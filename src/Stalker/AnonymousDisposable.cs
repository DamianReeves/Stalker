using System;
using System.Threading;

namespace Stalker {
    internal sealed class AnonymousDisposable: IDisposable {
        private volatile Action _dispose;

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// 
        /// </summary>
        public bool IsDisposed {
            get {
                return this._dispose == null;
            }
        }

        /// <summary>
        /// Constructs a new disposable with the given action used for disposal.
        /// 
        /// </summary>
        /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
        public AnonymousDisposable(Action dispose) {
            if (dispose == null) throw new ArgumentNullException("dispose");
            this._dispose = dispose;
        }

        /// <summary>
        /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
        /// 
        /// </summary>
        public void Dispose() {
            Action action = Interlocked.Exchange<Action>(ref this._dispose, (Action)null);
            if (action == null)
                return;
            action();
        }
    }
}