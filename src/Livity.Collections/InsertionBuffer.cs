using System;
using System.Collections;
using System.Collections.Generic;

namespace Livity.Collections
{
	/// <summary>
	/// An immutable list with support for
	/// fast appending semantics.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IAppendableList<T> : IImmutableList<T>
	{
		bool IsAppendable { get; }
		IImmutableList<T> Append(T element);
	}

	public static class AppendableList
	{
		public static bool IsAppendable<T>(IImmutableList<T> piece)
		{
			var appendablePiece = piece as IAppendableList<T>;
			return appendablePiece != null && appendablePiece.IsAppendable;
		}
	}

	public class InsertionBuffer<T>
	{
		private readonly List<T> _buffer = new List<T>();

		public IAppendableList<T> Insert(T value)
		{
			var collection = new AppendableList(_buffer, _buffer.Count, 1);
			_buffer.Add(value);
			return collection;
		}

		class AppendableList : IAppendableList<T>
		{
			private readonly List<T> _buffer;
			private readonly int _count;
			private readonly int _index;

			public AppendableList(List<T> buffer, int index, int count)
			{
				_buffer = buffer;
				_index = index;
				_count = count;
			}

			public int Count
			{
				get { return _count; }
			}

			object IImmutableList.this[int index]
			{
				get { return this[index]; }
			}

			public T this[int index]
			{
				get { return _buffer[_index + index]; }
			}

			public bool IsAppendable
			{
				get { return _index  + _count == _buffer.Count; }
			}

			public void CopyTo(T[] destination, int destinationIndex, int sourceIndex, int length)
			{
				_buffer.CopyTo(_index + sourceIndex, destination, destinationIndex, length);
			}

			public IImmutableList<T> GetRange(int index, int length)
			{
				return new AppendableList(_buffer, _index + index, length);
			}

			public IImmutableList<T> Append(T value)
			{
				if (!IsAppendable)
					throw new InvalidOperationException();
				_buffer.Add(value);
				return new AppendableList(_buffer, _index, _count + 1);
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<T>)this).GetEnumerator();
			}
		}
	}
}
