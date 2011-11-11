using System.ComponentModel;

namespace Stalker.Prey
{
	/// <summary>
	/// Specialization of INotifyPropertyChanged to introduce an OnPropertyChanged method
	/// that can be called to raise the PropertyChanged event.  This is necessary since
	/// events can only be raised by the class defining the event.
	/// </summary>
	public interface INotifyPropertyChangedAmendment : INotifyPropertyChanged
	{
		void OnPropertyChanged(PropertyChangedEventArgs args);
	}
}
