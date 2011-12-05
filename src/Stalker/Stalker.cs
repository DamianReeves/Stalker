using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace Stalker {
    public static class Stalker {        
        public static IStalker Stalk<T>(T watched) {
            return new Stalker<T>();
        }

        private static void Stalk(INotifyCollectionChanged collection) {            
            
        }        
    }

    internal class Stalker<T> : IStalker {
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        public void Stalk(INotifyCollectionChanged collection) {

        }

        public IStalker Watch<TResult>(Expression<Func<T,TResult>> propertyExpression) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            if (!_disposable.IsDisposed) {
                _disposable.Dispose();
            }
        }
    }

    public interface IStalker: IDisposable {
        
    }

    
}
