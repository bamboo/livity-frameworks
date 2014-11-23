using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Livity.Collections;
using Livity.Composition.Primitives;

namespace Livity.Composition.Hosting
{
	public class CompositionContainer : IExportProvider
	{
		readonly Cache<Type, Export[]> _exports;
		readonly Cache<Type, Lazy<object>> _parts;
		readonly IExportDefinitionProvider _exportDefinitionProvider;

		public CompositionContainer(Assembly assembly)
			: this(new TypeCatalog(assembly))
		{
		}

		public CompositionContainer(params Assembly[] assemblies)
			: this(TypeCatalog.For(assemblies))
		{
		}

		public CompositionContainer(IEnumerable<Assembly> assemblies)
			: this(TypeCatalog.For(assemblies))
		{
		}

		public CompositionContainer(IExportDefinitionProvider definitionProvider)
		{
			_exportDefinitionProvider = definitionProvider;
			_exports = new Cache<Type, Export[]>(CreateExports);
			_parts = new Cache<Type, Lazy<object>>(PreparePart);
			AddExportedValue<IExportProvider>(this);
		}

		public void AddExportedValue<T>(T value, params object[] metadata)
		{
			AddExport(new Export(new ExportDefinition(typeof(T), value.GetType(), () => metadata ?? EmptyArray.Of<object>()), () => value));
		}

		void AddExport(Export export)
		{
			lock (_exports)
				_exports.Add(export.Definition.ContractType, new[] {export});
		}

		public T GetExportedValue<T>()
		{
			return (T) GetExportedValue(typeof(T));
		}

		public object GetExportedValue(Type contractType)
		{
			var export = GetExport(contractType);
			if (export == null)
				throw CompositionErrors.NoExportFoundError(contractType);
			return export.Value;
		}

		Export GetExport(Type contractType)
		{
			return GetExports(contractType).SingleOrDefault();
		}

		public IEnumerable<Export> GetExports(Type contractType)
		{
			lock (_exports)
				return DoGetExports(contractType);
		}

		IEnumerable<Export> DoGetExports(Type contractType)
		{
			return _exports.GetOrCreate(contractType);
		}

		Export[] CreateExports(Type contractType)
		{
			return _exportDefinitionProvider
				.GetExports(contractType)
				.Select(e => new Export(e, FactoryFor(e)))
				.ToArray();
		}

		Func<object> FactoryFor(ExportDefinition exportDefinition)
		{
			return AccessorFor(exportDefinition, GetPartFor(exportDefinition.Implementation));
		}

		Lazy<object> GetPartFor(Type implementation)
		{
			lock (_parts)
				return _parts.GetOrCreate(implementation);
		}

		Lazy<object> PreparePart(Type implementation)
		{
			return Lazy.From(() => InstantiatePart(implementation));
		}

		object InstantiatePart(Type implementation)
		{
			lock (this)
				return DoInstantiatePart(implementation);
		}

		object DoInstantiatePart(Type implementation)
		{
			_compositionState = _compositionState.Instantiating(implementation);
			try
			{
				var importingConstructor = ImportingConstructorOf(implementation);
				var part = importingConstructor != null
					? CreateInstanceThrough(importingConstructor)
					: Activator.CreateInstance(implementation);
				this.SatisfyImportsOnce(part);
				_compositionState = _compositionState.Instantiated(implementation);
				return part;
			}
			catch (Exception)
			{
				_compositionState = CompositionState.Idle;
				throw;
			}
		}

		CompositionState _compositionState = CompositionState.Idle;

		class CompositionState
		{
			public static readonly CompositionState Idle = new CompositionState();

			public virtual CompositionState Instantiating(Type implementation)
			{
				return new CompositionStateInstantiating(implementation);
			}

			public virtual CompositionState Instantiated(Type implementation)
			{
				throw new InvalidOperationException();
			}
		}

		class CompositionStateInstantiating : CompositionState
		{
			readonly List<Type> _types = new List<Type>();

			public CompositionStateInstantiating(Type implementation)
			{
				Add(implementation);
			}

			override public CompositionState Instantiating(Type implementation)
			{
				Add(implementation);
				var cycleIndex = _types.IndexOf(implementation, 0, _types.Count - 1);
				if (cycleIndex >= 0) 
						throw new InvalidOperationException("A composition cycle was detected: `" + "' -> `".Join(_types.Skip(cycleIndex)) + "'.");
				return this;
			}

			override public CompositionState Instantiated(Type implementation)
			{
				var lastIndex = _types.Count - 1;
				if (_types[lastIndex] != implementation)
					throw new InvalidOperationException();
				if (_types.Count == 1)
					return Idle;
				_types.RemoveAt(lastIndex);
				return this;
			}

			void Add(Type implementation)
			{
				_types.Add(implementation);
			}
		}

		Func<object> AccessorFor(ExportDefinition exportDefinition, Lazy<object> part)
		{
			return () =>
			{
				try
				{
					return part.Value;
				}
				catch (Exception e)
				{
					throw new CompositionException(e,
						new CompositionError(exportDefinition.ContractType,
							string.Format("Failed to create `{0}' to satisfy `{1}'!",
								exportDefinition.Implementation,
								exportDefinition.ContractType)));
				}
			};
		}

		object CreateInstanceThrough(ConstructorInfo importingConstructor)
		{
			return importingConstructor.Invoke(ExportedValuesFor(importingConstructor).ToArray());
		}

		IEnumerable<object> ExportedValuesFor(ConstructorInfo importingConstructor)
		{
			return importingConstructor.GetParameters().Select(ExportedValueForParameter);
		}

		private object ExportedValueForParameter(ParameterInfo p)
		{
			var parameterType = p.ParameterType;
			return parameterType.IsArray
				? ExportedValueArrayOf(parameterType.GetElementType())
				: GetExportedValue(parameterType);
		}

		private Array ExportedValueArrayOf(Type contractType)
		{
			return GetExportedValues(contractType).ToArrayOf(contractType);
		}

		private IEnumerable<object> GetExportedValues(Type contractType)
		{
			return GetExports(contractType).Select(it => it.Value);
		}

		static ConstructorInfo ImportingConstructorOf(Type implementation)
		{
			return implementation.InstanceConstructors().SingleOrDefault(IsImportingConstructor);
		}

		static bool IsImportingConstructor(ConstructorInfo c)
		{
			return Attribute.IsDefined(c, typeof(ImportingConstructor));
		}

		public IImmutableList<object> GetInstantiatedParts()
		{
			lock (_parts)
				return _parts.Values.Where(_ => _.HasValue).Select(_ => _.Value).ToImmutableList();
		}
	}
}