using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization
{
	internal static class SerializableExtensions
	{
		public static bool IsBytesCastable(this Type type)
		{
			if (type.IsPrimitive) return true;

			if (type.IsEnum) return true;

			if (type.IsLayoutSequential || type.IsExplicitLayout) return true;

			return false;
		}
	}
}
