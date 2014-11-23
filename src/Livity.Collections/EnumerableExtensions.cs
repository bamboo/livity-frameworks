using System;
using System.Collections.Generic;
using System.Linq;

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

		public static Array ToArrayOf(this IEnumerable<object> enumerable, Type contractType)
		{
			var source = enumerable.ToArray();
			var result = Array.CreateInstance(contractType, source.Length);
			Array.Copy(source, result, source.Length);
			return result;
		}
	}
}