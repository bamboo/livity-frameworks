using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Livity.Collections.Tests
{
	[TestFixture]
	public class ImmutableListTest
	{
		const string TestString = "0123456789";

		public static readonly TestCaseData[] TestRanges = new[]
		{
		  new TestCaseData(0, 0),
		  new TestCaseData(0, 1),
		  new TestCaseData(0, 2),
		  new TestCaseData(1, 0),
		  new TestCaseData(1, 1),
		  new TestCaseData(1, 2),
		  new TestCaseData(9, 0),
		  new TestCaseData(9, 1),
		  new TestCaseData(8, 2)
		};

		[Test]
		[TestCaseSource("TestRanges")]
		public void EnumerableForRange(int start, int length)
		{
			var subject = (IEnumerable<char>)ImmutableList.ForRange(ImmutableList.ForString(TestString), start, length);
			Assert.AreEqual(TestString.Substring(start, length), new string(subject.ToArray()));
		}

		[Test]
		[TestCaseSource("TestRanges")]
		public void CopyToForRange(int start, int length)
		{
			var subject = ImmutableList.ForRange(ImmutableList.ForString(TestString), start, length);
			var array = new char[length];
			subject.CopyTo(array, 0, 0, length);
			Assert.AreEqual(TestString.Substring(start, length), new string(array));
		}

		[Test]
		[TestCaseSource("TestRanges")]
		public void CopyToForList(int start, int length)
		{
			var subject = ImmutableList.ForList(TestString.ToList());
			var array = new char[length];
			subject.CopyTo(array, 0, start, length);
			Assert.AreEqual(TestString.Substring(start, length), new string(array));
		}
	}
}