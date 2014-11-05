using System;
using System.Collections.Generic;

namespace Livity.Collections
{
	public static class EnumerableExtensions
	{
		public static string ToCommaSeparatedList(this IEnumerable<string> source)
		{
			return ", ".Join(source);
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			return new HashSet<T>(source);
		}

		public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var element in source)
			{
				action(element);
				yield return element;
			}
		}
	}
}