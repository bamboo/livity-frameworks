using System;
using System.Collections.Generic;
using Livity.Collections;

namespace Livity.Composition.Primitives
{
	public class Export : Lazy<object>, IMetadataProvider
	{
		public Export(ExportDefinition definition, Func<object> valueFactory) : base(valueFactory)
		{
			Definition = definition;
		}

		public ExportDefinition Definition
		{
			get; private set;
		}

		public IEnumerable<object> Metadata
		{
			get { return Definition.Metadata; }
		}
	}
}