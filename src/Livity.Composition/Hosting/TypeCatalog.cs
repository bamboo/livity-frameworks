using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Livity.Collections;
using Livity.Composition.Primitives;

namespace Livity.Composition.Hosting
{
	public class TypeCatalog : IExportDefinitionProvider
	{
		public static IExportDefinitionProvider For(IEnumerable<Assembly> assemblies)
		{
			return new AggregateCatalog(AssemblyCatalogsFor(assemblies));
		}

		public static IExportDefinitionProvider For(params Type[] types)
		{
			return new TypeCatalog(types);
		}

		static IEnumerable<IExportDefinitionProvider> AssemblyCatalogsFor(IEnumerable<Assembly> assemblies)
		{
			return assemblies
				.Where(assembly => assembly.IsSafeForComposition())
				.Distinct()
				.Select(assembly => (IExportDefinitionProvider) new TypeCatalog(assembly));
		}

		readonly Lazy<Dictionary<Type, ExportDefinition[]>> _exports;

		public TypeCatalog(Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException();
			_exports = Lazy.From(() => ComputeExportsFrom(assembly.GetTypes()));
		}

		public TypeCatalog(IEnumerable<Type> types)
		{
			if (types == null) throw new ArgumentNullException();
			_exports = Lazy.From(() => ComputeExportsFrom(types));
		}

		public IEnumerable<ExportDefinition> GetExports(Type contractType)
		{
			ExportDefinition[] exportsDefinition;
			return _exports.Value.TryGetValue(contractType, out exportsDefinition)
				? exportsDefinition
				: NoExportsDefinition;
		}

		Dictionary<Type, ExportDefinition[]> ComputeExportsFrom(IEnumerable<Type> types)
		{
			return types
				.Where(t => !t.IsAbstract)
				.SelectMany(ExportsFromType)
				.Where(e => e != null)
				.GroupBy(e => e.ContractType)
				.ToDictionary(e => e.Key, e => e.ToArray());
		}

		IEnumerable<ExportDefinition> ExportsFromType(Type implementation)
		{
			foreach (var attribute in ExportAttributesFrom(implementation))
				yield return new ExportDefinition(attribute.ContractType ?? implementation, implementation);

			foreach (var baseType in implementation.BaseTypes())
				foreach (var inheritedAttribute in InheritedExportAttributesFrom(baseType))
					yield return new ExportDefinition(inheritedAttribute.ContractType ?? baseType, implementation);
		}

		static IEnumerable<ExportAttribute> ExportAttributesFrom(Type implementation)
		{
			return CustomAttribute<ExportAttribute>.AllFrom(implementation);
		}

		static IEnumerable<InheritedExportAttribute> InheritedExportAttributesFrom(Type baseType)
		{
			return CustomAttribute<InheritedExportAttribute>.AllFrom(baseType);
		}

		static readonly ExportDefinition[] NoExportsDefinition = EmptyArray.Of<ExportDefinition>();
	}
}