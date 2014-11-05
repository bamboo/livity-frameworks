using System;
using System.Collections.Generic;
using System.Linq;

namespace Livity.Composition.Primitives
{
	public interface IExportProvider
	{
		IEnumerable<Export> GetExports(Type contractType);
	}

	public static class ExportProviderExtensions
	{
		public static IEnumerable<Export> GetExportsWhereMetadata<T>(this IExportProvider provider, Func<T, bool> predicate, Type contractType)
		{
			return provider.GetExports(contractType).Where(e => e.Metadata.OfType<T>().Any(predicate));
		}

		public static T CreateNonShared<T>(this IExportProvider provider) where T: new()
		{
			var part = new T();
			provider.SatisfyImportsOnce(part);
			return part;
		}

		public static void SatisfyImportsOnce(this IExportProvider provider, object part)
		{
			foreach (var import in ImportsOf(part))
				Satisfy(import, part, provider);
		}

		static void Satisfy(ImportDefinition importDefinition, object part, IExportProvider provider)
		{
			var exports = GetExportsSatisfying(importDefinition, provider);
			Validate(importDefinition, exports);
			importDefinition.SatisfyWith(exports, part);
		}

		static void Validate(ImportDefinition importDefinition, Export[] exports)
		{
			switch (importDefinition.Cardinality)
			{
				case ImportCardinality.One:
					if (exports.Length == 0)
						throw CompositionErrors.NoExportFoundError(importDefinition.ContractType);
					if (exports.Length != 1)
						throw CompositionErrors.TooManyExportsError(importDefinition, exports);
					break;
			}
		}

		static Export[] GetExportsSatisfying(ImportDefinition importDefinition, IExportProvider provider)
		{
			return provider
				.GetExports(importDefinition.ContractType)
				.Where(importDefinition.IsSatisfiableBy)
				.ToArray();
		}

		static IEnumerable<ImportDefinition> ImportsOf(object value)
		{
			return ImportDefinitionProvider.ImportsOf(value.GetType());
		}

		public static IEnumerable<TExport> GetExportedValuesWithoutMetadataOfType<TMetadata, TExport>(this IExportProvider exportProvider)
		{
			return exportProvider
				.GetExports(typeof(TExport))
				.Where(_ => !_.Metadata.OfType<TMetadata>().Any())
				.Select(_ => _.Value)
				.Cast<TExport>();
		}
	}

}