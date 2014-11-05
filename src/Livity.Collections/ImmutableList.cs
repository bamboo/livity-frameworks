using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Livity.Collections
{
	public interface IImmutableList : IEnumerable
	{
		int Count { get; }
		object this[int index] { get; }
	}

	public interface IImmutableList<T> : IImmutableList, IEnumerable<T>
	{
		new T this[int index] { get; }
		IImmutableList<T> GetRange(int index, int length);
		void CopyTo(T[] destination, int destinationIndex, int sourceIndex, int length);
	}

	public static class ImmutableList
	{
		public static IImmutableList<T> Empty<T>()
		{
			return EmptyList<T>.Instance;
		}

		public static IImmutableList<T> Cast<T>(this IImmutableList source)
		{
			return ((IEnumerable)source).Cast<T>().ToImmutableList();
		}

		public static bool IsNullOrEmpty<T>(this IImmutableList<T> list)
		{
			return list == null || list.Count == 0;
		}

		public static IImmutableList<T> Of<T>(params T[] array)
		{
			return ForArray(array);
		}

		public static IImmutableList<T> ForList<T>(List<T> list)
		{
			return list.Count == 0 ? Empty<T>() : new OverList<T>(list);
		}

		public static IImmutableList<T> ForArray<T>(T[] array)
		{
			return array.Length == 0 ? Empty<T>() : new OverArray<T>(array);
		}

		public static IImmutableList<char> ForString(string s)
		{
			return s.Length == 0 ? Empty<char>() : new OverString(s);
		}

		public static IImmutableList<TResult> ConvertAll<T, TResult>(this IImmutableList<T> source, Func<T, TResult> resultSelector)
		{
			if (source.Count == 0)
				return Empty<TResult>();

			var result = new TResult[source.Count];
			for (var i = 0; i < result.Length; ++i)
				result[i] = resultSelector(source[i]);
			return ForArray(result);
		}

		public static IImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source)
		{
			return ForList(source.ToList());
		}

		public static IImmutableList<char> ToImmutableList(this string source)
		{
			return ForString(source);
		}

		public static IImmutableList<T> ForRange<T>(IImmutableList<T> source, int start, int length)
		{
			return length == 0 ? Empty<T>() : new OverRange<T>(source, start, length);
		}

		public static IImmutableList<T> Merge<T>(this IEnumerable<IImmutableList<T>> sources)
		{
			var nonEmpty = ListSegment.NonEmptySegmentsFrom(sources);
			if (nonEmpty.Count == 0)
				return Empty<T>();

			if (nonEmpty.Count == 1)
				return nonEmpty[0].List;

			var segments = nonEmpty.ToArray();
			return new CompositeImmutableList<T>(segments);
		}

		public static Pair<IImmutableList<T>, IImmutableList<T>> SplitAt<T>(this IImmutableList<T> source, int position)
		{
			var previous = source.GetRange(0, position);
			var next = source.GetRange(position, source.Count - position);
			return Pair.Of(previous, next);
		}

		public static T[] ToArray<T>(this IImmutableList<T> source, int position, int length)
		{
			if (position + length > source.Count)
				throw new ArgumentOutOfRangeException();

			var destination = new T[length];
			source.CopyTo(destination, 0, position, length);
			return destination;
		}

		public static T[] ToArray<T>(this IImmutableList<T> source)
		{
			return source.ToArray(0, source.Count);
		}

		public static TResult[] ToArray<T, TResult>(this IImmutableList<T> source, Func<T, TResult> resultSelector)
		{
			var result = new TResult[source.Count];
			var i = 0;
			foreach (var element in source)
				result[i++] = resultSelector(element);
			return result;
		}

		public static int FindIndex<T>(this IImmutableList<T> source, Predicate<T> predicate)
		{
			var i = 0;
			foreach (var e in source)
				if (predicate(e))
					return i;
				else
					i++;
			return -1;
		}

		public static int FindLastIndex<T>(this IImmutableList<T> source, Predicate<T> predicate)
		{
			var i = source.Count - 1;
			while (i >= 0)
				if (predicate(source[i]))
					return i;
				else
					i--;
			return -1;
		}

		public static int IndexOf<T>(this IImmutableList<T> source, T element)
		{
			var i = 0;
			foreach (var e in source)
				if (Equals(element, e))
					return i;
				else
					i++;
			return -1;
		}

		class OverString : IImmutableList<char>
		{
			readonly string _s;

			public OverString(string s)
			{
				_s = s;
			}

			public int Count
			{
				get { return _s.Length; }
			}

			object IImmutableList.this[int index]
			{
				get { return _s[index]; }
			}

			public void CopyTo(char[] destination, int destinationIndex, int sourceIndex, int length)
			{
				_s.CopyTo(sourceIndex, destination, destinationIndex, length);
			}

			public IImmutableList<char> GetRange(int index, int length)
			{
				return new OverString(_s.Substring(index, length));
			}

			public char this[int index]
			{
				get { return _s[index]; }
			}

			public IEnumerator<char> GetEnumerator()
			{
				return _s.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		class OverArray<T> : IImmutableList<T>
		{
			readonly T[] _array;

			public OverArray(T[] array)
			{
				_array = array;
			}

			public int Count
			{
				get { return _array.Length; }
			}

			object IImmutableList.this[int index]
			{
				get { return _array[index]; }
			}

			public T this[int index]
			{
				get { return _array[index]; }
			}

			public void CopyTo(T[] destination, int destinationIndex, int sourceIndex, int length)
			{
				Array.Copy(_array, sourceIndex, destination, destinationIndex, length);
			}

			public IImmutableList<T> GetRange(int index, int length)
			{
				return new OverRange<T>(this, index, length);
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return ((IEnumerable<T>)_array).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)_array).GetEnumerator();
			}

			public override string ToString()
			{
				return ImmutableList.ToString(_array);
			}
		}

		class OverRange<T> : IImmutableList<T>
		{
			readonly IImmutableList<T> _list;
			readonly int _index;
			readonly int _count;

			public OverRange(IImmutableList<T> list, int index, int count)
			{
				_list = list;
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
				get { return _list[_index + index]; }
			}

			public void CopyTo(T[] destination, int destinationIndex, int sourceIndex, int length)
			{
				_list.CopyTo(destination, destinationIndex, _index + sourceIndex, length);
			}

			public IImmutableList<T> GetRange(int index, int length)
			{
				return new OverRange<T>(_list, _index + index, length);
			}

			public IEnumerator<T> GetEnumerator()
			{
				for (var i = 0; i < _count; ++i)
					yield return this[i];
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		class OverList<T> : IImmutableList<T>
		{
			readonly List<T> _list;

			public OverList(List<T> list)
			{
				_list = list;
			}

			public int Count
			{
				get { return _list.Count; }
			}

			object IImmutableList.this[int index]
			{
				get { return _list[index]; }
			}

			public IImmutableList<T> GetRange(int index, int length)
			{
				return ForRange(this, index, length);
			}

			public void CopyTo(T[] destination, int destinationIndex, int sourceIndex, int length)
			{
				_list.CopyTo(sourceIndex, destination, destinationIndex, length);
			}

			public T this[int index]
			{
				get { return _list[index]; }
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public override string ToString()
			{
				return ImmutableList.ToString(_list);
			}
		}

		class EmptyList<T> : IImmutableList<T>
		{
			public static readonly IImmutableList<T> Instance = new EmptyList<T>();

			public int Count { get { return 0; } }

			object IImmutableList.this[int index]
			{
				get { throw new IndexOutOfRangeException(); }
			}

			public IImmutableList<T> GetRange(int index, int length)
			{
				if (index == 0 && length == 0)
					return this;
				throw new IndexOutOfRangeException();
			}

			public void CopyTo(T[] destination, int destinationIndex, int sourceIndex, int length)
			{
				if (sourceIndex == 0 && length == 0)
					return;
				throw new IndexOutOfRangeException();
			}

			public T this[int index]
			{
				get { throw new IndexOutOfRangeException(); }
			}

			public IEnumerator<T> GetEnumerator()
			{
				yield break;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public override string ToString()
			{
				return "[]";
			}
		}

		internal static string ToString<T>(IEnumerable<T> list)
		{
			var stringBuilder = new StringBuilder("[");
			foreach (var item in list)
			{
				if (stringBuilder.Length > 1)
					stringBuilder.Append(", ");
				stringBuilder.Append(item);
			}
			return stringBuilder.Append("]").ToString();
		}
	}
}