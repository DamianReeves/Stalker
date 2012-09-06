using System.ComponentModel;

namespace Stalker {
    public class NotifyingObject : INotifyPropertyChanged, INotifyPropertyChanging {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event PropertyChangingEventHandler PropertyChanging = delegate { };

        protected virtual void OnPropertyChanged(string propertyName) {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanging(string propertyName) {
            OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e) {
            PropertyChangingEventHandler handler = PropertyChanging;
            if (handler != null) handler(this, e);
        }
    }
}