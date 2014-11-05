using System;
using System.Collections.Generic;
using System.Linq;

namespace Livity.Collections
{
	public class Cache<TKey, TValue>
	{
		readonly Func<TKey, TValue> _factory;
		readonly Dictionary<TKey, TValue> _values;

		public Cache(Func<TKey, TValue> factory)
		{
			_factory = factory;
			_values = new Dictionary<TKey, TValue>();
		}

		public ICollection<TValue> Values
		{
			get { return _values.Values; }
		}

		public void Add(TKey key, TValue value)
		{
			_values.Add(key, value);
		}

		public void Clear()
		{
			_values.Clear();
		}

		public TValue GetOrCreate(TKey key)
		{
			TValue existing;
			if (_values.TryGetValue(key, out existing))
				return existing;
			TValue newValue = _factory(key);
			_values.Add(key, newValue);
			return newValue;
		}

		public void RemoveWhere(Func<TKey, bool> predicate)
		{
			RemoveRange(_values.Keys.Where(predicate).ToArray());
		}

		public void RemoveRange(TKey[] keys)
		{
			foreach (var key in keys)
				Remove(key);
		}

		public bool Remove(TKey key)
		{
			return _values.Remove(key);
		}
	}
}