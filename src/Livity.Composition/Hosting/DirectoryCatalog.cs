using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Livity.Composition.Primitives;

namespace Livity.Composition.Hosting
{
	public class DirectoryCatalog : IExportDefinitionProvider
	{
		public static Assembly[] AllAssembliesIn(string directory)
		{
			return Directory.GetFiles(directory, "*.dll").Select(s => Assembly.LoadFrom(s)).ToArray();
		}

		readonly IExportDefinitionProvider _catalog;

		public DirectoryCatalog(string directory)
		{
			_catalog = TypeCatalog.For(AllAssembliesIn(directory));
		}

		public IEnumerable<ExportDefinition> GetExports(Type contractType)
		{
			return _catalog.GetExports(contractType);
		}
	}
}