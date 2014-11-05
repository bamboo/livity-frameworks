using System.Linq;
using Livity.Collections;
using Livity.Composition.Primitives;
using NUnit.Framework;

namespace Livity.Composition.Tests
{
	[TestFixture]
	public class ImportDefinitionProviderTest
	{
		[Test]
		public void ImportsAreInherited()
		{
			var import = ImportDefinitionProvider.ImportsOf(typeof(ClassWithInheritedImport)).Single();
			Assert.AreEqual(typeof(IService), import.ContractType);
		}

		class ClassWithInheritedImport : AbstractClassWithImport {}

		abstract class AbstractClassWithImport
		{
			[Import]
			IService Service { get; set; }
		}

		[Test]
		public void LazyImportWithMetadata()
		{
			var import = ImportDefinitionProvider.ImportsOf(typeof(ClassWithLazyImportWithMetadata)).Single();
			Assert.AreEqual(typeof(IService), import.ContractType);
			Assert.AreEqual(ImportCardinality.One, import.Cardinality);
		}

		[Test]
		public void LazyImport()
		{
			var import = ImportDefinitionProvider.ImportsOf(typeof(ClassWithLazyImport)).Single();
			Assert.AreEqual(typeof(IService), import.ContractType);
			Assert.AreEqual(ImportCardinality.One, import.Cardinality);
		}

		[Test]
		public void LazyImportMany()
		{
			var import = ImportDefinitionProvider.ImportsOf(typeof(ClassWithLazyImportMany)).Single();
			Assert.AreEqual(typeof(IService), import.ContractType);
			Assert.AreEqual(ImportCardinality.Many, import.Cardinality);
		}

		public class ClassWithLazyImportWithMetadata
		{
			[Import]
			public Lazy<IService, IServiceMetadata> Service;
		}

		public class ClassWithLazyImport
		{
			[Import]
			public Lazy<IService> Service;
		}

		public class ClassWithLazyImportMany
		{
			[ImportMany]
			public Lazy<IService, IServiceMetadata>[] Services;
		}

		public interface IService
		{
		}

		public interface IServiceMetadata
		{
		}
	}
}
