using System;

namespace Livity.Collections
{
	public struct CircularIterator<T>
	{
		readonly IImmutableList<T> _list;
		readonly int _index;

		public CircularIterator(IImmutableList<T> list, int index = 0)
		{
			if (index < 0 || index > list.Count - 1)
				throw new ArgumentOutOfRangeException("index", index, string.Format("Expected a value between 0 and {0}.", list.Count - 1));
			_list = list;
			_index = index;
		}

		public T Value
		{
			get { return _list[_index]; }
		}

		public CircularIterator<T> Previous
		{
			get
			{
				var previousIndex = _index == 0
					? _list.Count - 1
					: _index - 1;
				return new CircularIterator<T>(_list, previousIndex);
			}
		}

		public CircularIterator<T> Next
		{
			get
			{
				var nextIndex = (_index + 1) % _list.Count;
				return new CircularIterator<T>(_list, nextIndex);
			}
		}

		public int Index
		{
			get { return _index; }
		}
	}
}