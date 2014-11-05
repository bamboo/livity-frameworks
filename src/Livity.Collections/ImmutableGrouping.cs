using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Livity.Collections
{
	public static class ImmutableGrouping
	{
		public static ImmutableGrouping<TKey, TValue> Of<TKey, TValue>(TKey key, IEnumerable<TValue> values)
		{
			return new ImmutableGrouping<TKey, TValue>(key, values);
		}
	}

	public struct ImmutableGrouping<TKey, TValue> : IGrouping<TKey, TValue>
	{
		readonly TKey _key;
		readonly IImmutableList<TValue> _values;

		public ImmutableGrouping(TKey key, IEnumerable<TValue> values)
		{
			_key = key;
			_values = values.ToImmutableList();
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public TKey Key
		{
			get { return _key; }
		}
	}
}