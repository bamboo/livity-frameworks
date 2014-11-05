using System;
using Livity.Composition.Hosting;
using NUnit.Framework;

namespace Livity.Composition.Tests
{
	public static class ExceptionExtensions
	{
		public static Exception MostInnerException(this Exception e)
		{
			return e.InnerException != null ? e.InnerException.MostInnerException() : e;
		}
	}

	[TestFixture]
	public class CompositionCyclesTest
	{
		[Test]
		[TestCase(typeof(UsesClassWithImportingConstructorCycle), typeof(ClassWithImportingConstructorCycle), typeof(ClassWithCycle))]
		[TestCase(typeof(UsesClassWithCycle), typeof(ClassWithCycle), typeof(ClassWithImportingConstructorCycle))]
		public void CanDetectCompositionCycleBetweenImportingConstructorAndImport(Type export, Type a, Type b)
		{
			var subject = new CompositionContainer(TypeCatalog.For(export, a, b));
			var e = Assert.Throws<CompositionException>(() => subject.GetExportedValue(export));
			Assert.AreEqual(string.Format("A composition cycle was detected: `{0}' -> `{1}' -> `{0}'.", a, b), e.MostInnerException().Message);
		}

		[Export]
		public class UsesClassWithCycle
		{
			[Import]
			public ClassWithCycle Cycle;
		}

		[Export]
		public class UsesClassWithImportingConstructorCycle
		{
			[Import]
			public ClassWithImportingConstructorCycle Cycle;
		}

		[Export]
		public class ClassWithCycle
		{
			[Import] public ClassWithImportingConstructorCycle Cycle;
		}

		[Export]
		public class ClassWithImportingConstructorCycle
		{
			public ClassWithCycle Cycle { get; set; }

			[ImportingConstructor]
			public ClassWithImportingConstructorCycle(ClassWithCycle cycle)
			{
				Cycle = cycle;
			}
		}
	}
}
