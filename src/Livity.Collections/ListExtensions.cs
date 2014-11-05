using System;
using System.Collections.Generic;

namespace Livity.Collections
{
	public static class ListExtensions
	{
		public static void InsertSorted<T>(this List<T> list, T element, IComparer<T> comparer)
		{
			var index = list.BinarySearch(element, comparer);
			if (index < 0)
				list.Insert(Math.Abs(index) - 1, element);
			else
				list.Insert(index, element);
		}

		public static void RemoveFrom<T>(this List<T> list, int index)
		{
			var count = list.Count;
			if (index >= count)
				return;
			list.RemoveRange(index, count - index);
		}
	}
}