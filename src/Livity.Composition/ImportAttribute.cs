using System;
using Livity.Composition.Primitives;

namespace Livity.Composition
{
	/// <summary>
	/// Imports a service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ImportAttribute : ContractAttribute
	{
		public ImportAttribute(Type contractType) : base(contractType) {}
		public ImportAttribute() : base(null) {}

		public virtual ImportCardinality Cardinality
		{
			get { return ImportCardinality.One; }
		}
	}
}