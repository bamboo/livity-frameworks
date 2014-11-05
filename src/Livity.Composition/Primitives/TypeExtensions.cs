using System;
using System.Collections.Generic;
using System.Reflection;

namespace Livity.Composition.Primitives
{
	static class TypeExtensions
	{
		private const BindingFlags InstanceMemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public static IEnumerable<Type> Hierarchy(this Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.BaseType;
			}
		}

		public static MemberInfo[] InstanceMembers(this Type type)
		{
			return type.GetMembers(InstanceMemberFlags);
		}

		public static ConstructorInfo[] InstanceConstructors(this Type type)
		{
			return type.GetConstructors(InstanceMemberFlags);
		}
	}
}