using System.Linq;
using NUnit.Framework;

namespace Livity.Collections.Tests
{
	[TestFixture]
	public class ImmutableListMergeTest
	{
		[Test]
		public void MergeEmptyReturnsEmpty()
		{
			Assert.AreSame(Empty, new[] {Empty}.Merge());
			Assert.AreSame(Empty, new[] {Empty, Empty}.Merge());
		}

		[Test]
		public void MergeOptimizesEmptiesAway()
		{
			var one = ImmutableList.Of(1);
			var two = ImmutableList.Of(2);
			var composite = (CompositeImmutableList<int>)new[] {Empty, one, Empty, two, Empty}.Merge();
			Assert.AreEqual(new[] {one, two}, composite.Lists.ToArray());
		}

		[Test]
		public void MergeAllowsIndexedAccess()
		{
			var one = ImmutableList.Of(1);
			var two = ImmutableList.Of(2);
			var subject = new[] { one, two }.Merge();
			Assert.AreEqual(one[0], subject[0]);
			Assert.AreEqual(two[0], subject[1]);
		}

		static IImmutableList<int> Empty
		{
			get { return ImmutableList.Empty<int>(); }
		}
	}
}