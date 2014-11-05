using System;
using System.Collections.Generic;

namespace Livity.Collections
{
	public struct Span
	{
		public static Span operator +(Span x, Span y)
		{
			if (x.End != y.Start)
				throw new ArgumentException("Can not add non contiguous Spans");

			return new Span(x.Start, x.Length + y.Length);
		}

		public static bool operator==(Span x, Span y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(Span x, Span y)
		{
			return !(x == y);
		}

		public readonly int Start;

		public readonly int Length;

		public Span(int start, int length)
		{
			Start = start;
			Length = length;
		}

		public int End
		{
			get { return Start + Length; }
		}

		public bool Contains(int position)
		{
			return position >= Start && position < End;
		}

		public override string ToString()
		{
			return string.Format("[{0}..{1})", Start, End);
		}

		public bool Equals(Span other)
		{
			return other.Start == Start && other.Length == Length;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof(Span)) return false;
			return Equals((Span)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Start * 397) ^ Length;
			}
		}
	}

	public static class Spans
	{
		public static IEnumerable<Span> MergeAdjacent(this IEnumerable<Span> spans)
		{
			var enumerator = spans.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var merged = enumerator.Current;
				while (enumerator.MoveNext())
				{
					var current = enumerator.Current;
					if (merged.End != current.Start)
					{
						yield return merged;
						merged = current;
					}
					else
						merged += current;
				}
				yield return merged;
			}
		}

		public static Span? IntersectionWith(this Span x, Span y)
		{
			var intersectionStart = Math.Max(x.Start, y.Start);
			var intersectionEnd = Math.Min(x.End, y.End);
			if (intersectionEnd <= intersectionStart) return null;
			return new Span(intersectionStart, intersectionEnd - intersectionStart);
		}
	}
}
