using NUnit.Framework;

namespace Livity.Composition.Tests
{
	[TestFixture]
	public class ImportingConstructorTest : TestWithCompositionContainer
	{
		[Test]
		public void GetExportedValueSatisfiesImportingConstructor()
		{
			Assert.AreSame(GetExportedValue<IService>(), GetExportedValue<ServiceWithImportingConstructor>().AService);
		}

		public interface IService
		{
		}

		[Export(typeof(IService))]
		public class SimpleService : IService
		{
		}

		[Export]
		public class ServiceWithImportingConstructor
		{
			[ImportingConstructor]
			ServiceWithImportingConstructor(IService service)
			{
				AService = service;
			}

			public IService AService { get; private set; }
		}


	}
}
