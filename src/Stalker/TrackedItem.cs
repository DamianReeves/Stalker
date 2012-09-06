using System;

namespace Stalker {
    public class TrackedItem: ITrackedItem, IDisposable {
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TrackedItem() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if(disposing) {
                Console.WriteLine("Is disposing");    
            }
            Console.WriteLine("Dispose called");
        }
    }
}