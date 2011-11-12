using System.ComponentModel;

namespace Stalker.Prey
{
	/// <summary>
	/// Static class containing a method that will be called after each property set.
	/// This method in outside of the Amendment<,> subclass to ensure that the amended type
	/// will not have a runtime dependency on Afterthought.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class NotificationAmender<T>
	{
		public static void OnPropertyChanged<TProperty>(INotifyPropertyChangedAmendment instance, string property, TProperty oldValue, TProperty value, TProperty newValue)
		{
			// Only raise property changed if the value of the property actually changed
			if ((oldValue == null ^ newValue == null) || (oldValue != null && !oldValue.Equals(newValue)))
				instance.OnPropertyChanged(new PropertyChangedEventArgs(property));
		}
	}

	/// <summary>
	/// Amendment class that instructs Afterthought to implement INotifyPropertyChanged for the target types.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NotificationAmendment<T> : Afterthought.Amendment<T, INotifyPropertyChangedAmendment>
	{
		public NotificationAmendment()
		{
			var propertyChanged = Events.Add<PropertyChangedEventHandler>("PropertyChanged");

			// Implement INotifyPropertyChanged, specifying the PropertyChanged event
			Implement<INotifyPropertyChanged>(
				propertyChanged
			);
			// Implement INotifyPropertyChangedAmendment, specifying a method that raises the PropertyChanged event
			Implement<INotifyPropertyChangedAmendment>(
				Methods.Raise(propertyChanged, "OnPropertyChanged")
			);

			// Raise Property Changed
			Properties
				.Where(p => p.PropertyInfo.CanRead && p.PropertyInfo.CanWrite && p.PropertyInfo.GetSetMethod().IsPublic)
				.AfterSet(NotificationAmender<T>.OnPropertyChanged);
		}
	}
}
