using System;

namespace Livity.Composition
{
	[AttributeUsage(AttributeTargets.Constructor)]
	public class ImportingConstructor : Attribute
	{
	}
}