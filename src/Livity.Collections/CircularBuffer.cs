namespace Livity.Collections
{
	public class CircularBuffer<T>
	{
		public struct Iterator
		{
			readonly CircularBuffer<T> _buffer;
			readonly int _index;

			internal Iterator(CircularBuffer<T> buffer, int index)
			{
				_buffer = buffer;
				_index = index;
			}

			public T Value
			{
				get { return _buffer._elements[_index]; }
			}

			public Iterator Previous
			{
				get
				{
					var previousIndex = _index == _buffer._start
						? _buffer._end - 1
						: _index - 1;
					return new Iterator(_buffer, previousIndex);
				}
			}

			public Iterator Next
			{
				get
				{
					var previousIndex = _index == _buffer._end - 1
						? _buffer._start
						: _index + 1;
					return new Iterator(_buffer, previousIndex);
				}
			}
		}

		readonly T[] _elements;
		int _start;
		int _end;

		public CircularBuffer(int capacity)
		{
			_elements = new T[capacity + 1];
		}

		public bool IsFull
		{
			get { return (_end + 1) % _elements.Length == _start; }
		}

		public bool IsEmpty
		{
			get { return _end == _start; }
		}

		public Iterator Add(T value)
		{
			var index = _end;
			_elements[index] = value;
			_end = (_end + 1) % _elements.Length;
			if (_end == _start)
				_start = (_start + 1) % _elements.Length;
			return new Iterator(this, index);
		}
	}
}