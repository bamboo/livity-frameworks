using System;
using System.Collections.Generic;

namespace Livity.Collections.Concurrent
{
	public class ConcurrentDictionary<TKey, TValue>
	{
		readonly Dictionary<TKey, TValue> _dictionary;

		public ConcurrentDictionary()
		{
			_dictionary = new Dictionary<TKey, TValue>();
		}

		public ConcurrentDictionary(IEqualityComparer<TKey> equalityComparer)
		{
			_dictionary = new Dictionary<TKey, TValue>(equalityComparer);
		}

		public IImmutableList<KeyValuePair<TKey, TValue>> Entries
		{
			get { lock (_dictionary) return _dictionary.ToImmutableList(); }
		}

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
		{
			lock (_dictionary)
			{
				TValue existing;
				if (_dictionary.TryGetValue(key, out existing))
					return existing;
				TValue newValue = factory(key);
				_dictionary.Add(key, newValue);
				return newValue;
			}
		}

		public void Add(TKey key, TValue value)
		{
			lock (_dictionary)
				_dictionary.Add(key, value);
		}

		public void Remove(TKey key)
		{
			lock (_dictionary)
				_dictionary.Remove(key);
		}
	}
}