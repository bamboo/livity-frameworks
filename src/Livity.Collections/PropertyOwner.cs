using System;
using Livity.Collections.Concurrent;

namespace Livity.Collections
{
	public interface IPropertyOwner
	{
		IPropertyCollection Properties { get; }
	}

	public interface IPropertyCollection
	{
		T GetOrCreateSingletonProperty<T>(Func<T> factory);
	}

	public static class PropertyOwnerExtensions
	{
		public static T GetOrCreateSingletonProperty<T>(this IPropertyOwner propertyOwner, Func<T> factory)
		{
			return propertyOwner.Properties.GetOrCreateSingletonProperty(factory);
		}
	}

	public class PropertyOwner : IPropertyOwner
	{
		public PropertyOwner()
		{
			Properties = new PropertyCollection();
		}

		public IPropertyCollection Properties
		{
			get; private set;
		}
	}

	public class PropertyCollection : IPropertyCollection
	{
		public T GetOrCreateSingletonProperty<T>(Func<T> factory)
		{
			return (T)_singletonProperties.GetOrAdd(typeof(T).TypeHandle, _ => factory());
		}

		readonly ConcurrentDictionary<RuntimeTypeHandle, object> _singletonProperties = new ConcurrentDictionary<RuntimeTypeHandle, object>(); 
	}
}
