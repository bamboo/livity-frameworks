using System.Collections;
using System.Collections.Generic;

namespace Livity.Collections
{
	public class MostRecentlyUsed<T> : IEnumerable<T>
	{
		readonly List<T> _items;
		readonly IEqualityComparer<T> _comparer;
		readonly int _capacity;

		public MostRecentlyUsed(IEnumerable<T> items, IEqualityComparer<T> comparer, int capacity)
		{
			_capacity = capacity;
			_comparer = comparer;
			_items = new List<T>(items);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
			Remove(item);
			_items.Insert(0, item);
			_items.RemoveFrom(_capacity);
		}

		void Remove(T item)
		{
			var index = _items.FindIndex(existing => _comparer.Equals(existing, item));
			if (index < 0) return;
			_items.RemoveAt(index);
		}
	}
}