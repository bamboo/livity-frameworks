using System;
using System.Linq;

namespace Livity.Composition.Primitives
{
	static class CompositionErrors
	{
		public static CompositionException NoExportFoundError(Type contractType)
		{
			return new CompositionException(
				new CompositionError(contractType, string.Format("Export `{0}' not found!", contractType)));
		}

		public static CompositionException TooManyExportsError(ImportDefinition importDefinition, Export[] exports)
		{
			return new CompositionException(
				new CompositionError(importDefinition.ContractType,
					string.Format("Too many exports for `{0}': `{1}'.", importDefinition.ContractType,
						exports.Select(e => e.Definition.Implementation.FullName).ToList())));
		}
	}
}