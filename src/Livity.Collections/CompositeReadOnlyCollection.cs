using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Livity.Collections
{
	public class CompositeImmutableList<T> : IImmutableList<T>
	{
		internal readonly ListSegment<T>[] ListSegments;

		internal CompositeImmutableList(ListSegment<T>[] listSegments)
		{
			ListSegments = listSegments;
		}

		public IEnumerable<IImmutableList<T>> Lists
		{
			get { return ListSegments.Select(_ => _.List); }
		}

		public Iterator this[int position]
		{
			get { return new Iterator(position, ListSegments, SegmentIndexForPosition(position)); }
		}

		public int Count
		{
			get { return SegmentCount == 0 ? 0 : LastListSegment.End; }
		}

		public int SegmentCount
		{
			get { return ListSegments.Length; }
		}

		protected IImmutableList<T> LastList
		{
			get { return LastListSegment.List; }
		}

		ListSegment<T> LastListSegment
		{
			get { return ListSegments[LastIndex]; }
		}

		protected int LastIndex
		{
			get { return ListSegments.Length - 1; }
		}

		T IImmutableList<T>.this[int index]
		{
			get { return this[index].GetValue(); }
		}

		object IImmutableList.this[int index]
		{
			get { return this[index].GetValue(); }
		}

		public T[] ToArray()
		{
			var result = new T[Count];
			var destinationIndex = 0;
			foreach (var segment in ListSegments)
			{
				var list = segment.List;
				var length = list.Count;
				list.CopyTo(result, destinationIndex, 0, length);
				destinationIndex += length;
			}
			return result;
		}

		public void CopyTo(T[] destination, int destinationIndex, int position, int length)
		{
			CheckPosition(position, Count - length);

			var segmentIndex = SegmentIndexForPosition(position);
			while (segmentIndex < SegmentCount)
			{
				var segment = ListSegments[segmentIndex];
				var positionInSegment = position - segment.Start;
				var remainingLengthInSegment = segment.Length - positionInSegment;
				var lengthToCopy = Math.Min(remainingLengthInSegment, length);
				segment.List.CopyTo(destination, destinationIndex, positionInSegment, lengthToCopy);
				length -= lengthToCopy;
				if (length <= 0)
					break;
				destinationIndex += lengthToCopy;
				position += lengthToCopy;
				++segmentIndex;
			}
		}

		public IImmutableList<T> GetRange(int index, int length)
		{
			return ImmutableList.ForRange(this, index, length);
		}

		protected void CheckPosition(int position, int maxValue)
		{
			if (position < 0 || position > maxValue)
				throw new ArgumentOutOfRangeException("position", position,
					string.Format("position must be between 0 and {0}", maxValue));
		}

		protected int SegmentIndexForPosition(int position, int min = 0)
		{
			var max = LastIndex;
			while (max >= min)
			{
				var pivot = (min + max) / 2;
				var partition = ListSegments[pivot];
				if (position >= partition.Start)
				{
					if (position < partition.End)
						return pivot;
					min = pivot + 1;
					continue;
				}
				max = pivot - 1;
			}
			throw new InvalidOperationException();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			var count = Count;
			if (count == 0)
				yield break;
			for (var current = this[0]; current.Position < count; current = current.Next)
				yield return current.GetValue();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}

		public struct Iterator
		{
			public readonly int Position;

			readonly ListSegment<T>[] _listSegments;
			readonly int _segmentIndex;

			internal Iterator(int position, ListSegment<T>[] listSegments, int segmentIndex)
			{
				Position = position;
				_listSegments = listSegments;
				_segmentIndex = segmentIndex;
			}

			public T GetValue()
			{
				var segment = ListSegment;
				var positionInSegment = Position - segment.Start;
				return segment.List[positionInSegment];
			}

			public Iterator Next
			{
				get
				{
					var segment = ListSegment;
					var newPosition = Position + 1;
					var segmentIndex = newPosition < segment.End ? _segmentIndex : _segmentIndex + 1;
					return new Iterator(newPosition, _listSegments, segmentIndex);
				}
			}

			public Iterator Previous
			{
				get
				{
					var newPosition = Position - 1;
					if (_segmentIndex >= _listSegments.Length)
						return new Iterator(newPosition, _listSegments, _segmentIndex - 1);

					var segment = ListSegment;
					var segmentIndex = newPosition >= segment.Start ? _segmentIndex : _segmentIndex - 1;
					return new Iterator(newPosition, _listSegments, segmentIndex);
				}
			}

			ListSegment<T> ListSegment
			{
				get { return _listSegments[_segmentIndex]; }
			}
		}
	}
}