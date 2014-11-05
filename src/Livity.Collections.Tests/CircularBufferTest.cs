using NUnit.Framework;

namespace Livity.Collections.Tests
{
	[TestFixture]
	public class CircularBufferTest
	{
		[Test]
		[TestCase(1, new[] {1, 2, 3}, new[] {3, 3, 3})]
		[TestCase(2, new[] {1, 2}, new[] {2, 1, 2, 1})]
		[TestCase(3, new[] {1, 2}, new[] {2, 1, 2, 1})]
		[TestCase(3, new[] {1, 2, 3}, new[] {3, 2, 1, 3, 2, 1})]
		public void PreviousIterator(int capacity, int[] elements, int[] expectedPrevious)
		{
			var subject = new CircularBuffer<int>(capacity);
			var iterator = default(CircularBuffer<int>.Iterator);
			foreach (var element in elements)
				iterator = subject.Add(element);
			foreach (var e in expectedPrevious)
			{
				Assert.AreEqual(e, iterator.Value);
				iterator = iterator.Previous;
			}
		}

		[Test]
		[TestCase(1, new[] {1, 2, 3}, new[] {3, 3, 3})]
		[TestCase(2, new[] {1, 2}, new[] {2, 1, 2})]
		[TestCase(3, new[] {1, 2, 3}, new[] {3, 1, 2, 3, 1, 2, 3})]
		public void NextIterator(int capacity, int[] elements, int[] expectedNext)
		{
			var subject = new CircularBuffer<int>(capacity);
			var iterator = default(CircularBuffer<int>.Iterator);
			foreach (var element in elements)
				iterator = subject.Add(element);
			foreach (var e in expectedNext)
			{
				Assert.AreEqual(e, iterator.Value);
				iterator = iterator.Next;
			}
		}

		[Test]
		[TestCase(3, 0, true)]
		[TestCase(3, 1, false)]
		[TestCase(3, 2, false)]
		public void IsEmpty(int capacity, int elements, bool isEmpty)
		{
			var subject = new CircularBuffer<int>(capacity);
			AddMany(elements, subject);
			Assert.AreEqual(isEmpty, subject.IsEmpty);
		}

		[Test]
		[TestCase(3, 0, false)]
		[TestCase(3, 1, false)]
		[TestCase(3, 2, false)]
		[TestCase(3, 3, true)]
		[TestCase(3, 4, true)]
		public void IsFull(int capacity, int elements, bool isFull)
		{
			var subject = new CircularBuffer<int>(capacity);
			AddMany(elements, subject);
			Assert.AreEqual(isFull, subject.IsFull);
		}

		static void AddMany(int elements, CircularBuffer<int> subject)
		{
			for (var i = 0; i < elements; ++i)
				subject.Add(i);
		}
	}
}