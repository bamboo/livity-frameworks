using System.Collections.Generic;
using NUnit.Framework;

namespace Livity.Collections.Tests
{
	[TestFixture]
	public class SpanTest
	{
		[Test]
		[TestCaseSource("IntersectionTests")]
		public void Intersection(Span x, Span y, Span? intersection)
		{
			Assert.AreEqual(intersection, x.IntersectionWith(y));
		}

		public static IEnumerable<TestCaseData> IntersectionTests()
		{
			yield return IntersectionTest(Span(0, 1), Span(0, 1), Span(0, 1));
			yield return IntersectionTest(Span(0, 1), Span(1, 1), null);
			yield return IntersectionTest(Span(1, 1), Span(0, 1), null);
			yield return IntersectionTest(Span(0, 2), Span(0, 1), Span(0, 1));
			yield return IntersectionTest(Span(0, 1), Span(0, 2), Span(0, 1));
		}

		static Span Span(int start, int length)
		{
			return new Span(start, length);
		}

		static TestCaseData IntersectionTest(Span x, Span y, Span? intersection)
		{
			return new TestCaseData(x, y, intersection);
		}
	}
}
