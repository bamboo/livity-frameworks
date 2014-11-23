using Livity.Composition.Hosting;
using NUnit.Framework;

namespace Livity.Composition.Tests
{
	public abstract class TestWithCompositionContainer
	{
		protected CompositionContainer Container;

		[SetUp]
		public void SetUp()
		{
			Container = new CompositionContainer(GetType().Assembly);
		}

		protected T GetExportedValue<T>()
		{
			return Container.GetExportedValue<T>();
		}
	}
}