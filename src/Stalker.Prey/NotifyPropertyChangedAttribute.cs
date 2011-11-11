using System;
using System.ComponentModel;

namespace Stalker.Prey
{
	/// <summary>
	/// Marker attribute indicating that a type should implement 
	/// <see cref="INotifyPropertyChanged"/> for all public properties.
	/// </summary>
	public class NotifyPropertyChangedAttribute : Attribute
	{ }
}
