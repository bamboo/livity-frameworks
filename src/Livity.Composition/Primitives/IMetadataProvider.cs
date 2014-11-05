using System.Collections.Generic;

namespace Livity.Composition.Primitives
{
	public interface IMetadataProvider
	{
		IEnumerable<object> Metadata { get; }
	}
}