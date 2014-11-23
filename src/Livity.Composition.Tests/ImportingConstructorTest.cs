using System.Linq;
using NUnit.Framework;

namespace Livity.Composition.Tests
{
	[TestFixture]
	public class ImportingConstructorTest : TestWithCompositionContainer
	{
		[Test]
		public void CanInstantiateUsingPrivateImportingConstructor()
		{
			Assert.AreSame(GetExportedValue<IService>(), GetExportedValue<ServiceWithImportingConstructor>().AService);
		}

		[Test]
		public void ArrayParametersAreTreatedAsImportMany()
		{
			CollectionAssert.AreEquivalent(
				Container.GetExports(typeof(IMany)).Select(it => it.Value),
				GetExportedValue<ImportingConstructorWithArrayParameter>().Many);
		}

		public interface IMany
		{
		}

		[Export(typeof(IMany))]
		public class Many1 : IMany
		{
		}

		[Export(typeof(IMany))]
		public class Many2 : IMany
		{
		}

		[Export]
		public class ImportingConstructorWithArrayParameter
		{
			public IMany[] Many { get; private set; }

			[ImportingConstructor]
			public ImportingConstructorWithArrayParameter(params IMany[] many)
			{
				Many = many;
			}
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
			private ServiceWithImportingConstructor(IService service)
			{
				AService = service;
			}

			public IService AService { get; private set; }
		}
	}
}