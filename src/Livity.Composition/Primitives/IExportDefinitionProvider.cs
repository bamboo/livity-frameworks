using System;
using System.Collections.Generic;

namespace Livity.Composition.Primitives
{
	public interface IExportDefinitionProvider
	{
		IEnumerable<ExportDefinition> GetExports(Type contractType);
	}
}