using System.Linq;
using Livity.Composition.Hosting;
using NUnit.Framework;

namespace Livity.Composition.Tests
{
	[TestFixture]
	public class PartsTest
	{
		[Test]
		public void HasValue()
		{
			var container = new CompositionContainer(TypeCatalog.For(typeof(Export1), typeof(Export2)));
			var e1 = container.GetExportedValue<Export1>();
			Assert.AreSame(e1, container.GetInstantiatedParts().Single());
		}

		[Export]
		public class Export1 {}

		[Export]
		public class Export2 {}
	}
}
