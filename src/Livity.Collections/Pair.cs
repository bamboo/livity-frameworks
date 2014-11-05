namespace Livity.Collections
{
	public static class Pair
	{
		public static Pair<TFirst, TSecond> Of<TFirst, TSecond>(TFirst previous, TSecond next)
		{
			return new Pair<TFirst, TSecond>(previous, next);
		}
	}

	public struct Pair<TFirst, TSecond>
	{
		public readonly TFirst First;
		public readonly TSecond Second;

		public Pair(TFirst first, TSecond second)
		{
			First = first;
			Second = second;
		}

		public override string ToString()
		{
			return string.Format("Pair(First: {0}, Second: {1})", First, Second);
		}
	}
}