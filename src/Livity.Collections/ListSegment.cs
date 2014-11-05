using System.Collections.Generic;

namespace Livity.Collections
{
	static class ListSegment
	{
		public static ListSegment<T> For<T>(IImmutableList<T> piece, int start)
		{
			return new ListSegment<T>(piece, start);
		}

		public static ListSegment<T>[] NonEmptySegmentsFrom<T>(ListSegment<T>[] spans)
		{
			var nonEmptyCount = NonEmptyCount(spans);
			return nonEmptyCount == spans.Length
				? spans
				: NonEmptySegmentsFrom(spans, nonEmptyCount);
		}

		public static ListSegment<T>[] NonEmptySegmentsFrom<T>(IEnumerable<ListSegment<T>> spans, int nonEmptyCount)
		{
			var nonEmpty = new ListSegment<T>[nonEmptyCount];

			var nonEmptyIndex = 0;
			foreach (var span in spans)
				if (span.Length > 0)
					nonEmpty[nonEmptyIndex++] = span;

			return nonEmpty;
		}

		public static int NonEmptyCount<T>(ListSegment<T>[] spans)
		{
			var nonEmptyCount = 0;
// ReSharper disable LoopCanBeConvertedToQuery
			foreach (var span in spans)
// ReSharper restore LoopCanBeConvertedToQuery
				if (span.Length > 0)
					++nonEmptyCount;
			return nonEmptyCount;
		}

		public static List<ListSegment<T>> NonEmptySegmentsFrom<T>(IEnumerable<IImmutableList<T>> sources)
		{
			var nonEmpty = new List<ListSegment<T>>();
			var offset = 0;
			foreach (var source in sources)
				if (source.Count > 0)
				{
					nonEmpty.Add(For(source, offset));
					offset += source.Count;
				}
			return nonEmpty;
		}
	}

	struct ListSegment<T>
	{
		public readonly IImmutableList<T> List;
		public readonly int Start;

		public ListSegment(IImmutableList<T> list, int start)
		{
			Start = start;
			List = list;
		}

		public int End
		{
			get { return Start + Length; }
		}

		public int Length
		{
			get { return List.Count; }
		}
	}
}
