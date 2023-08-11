using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CM3D2.Serialization
{
	// This extension is necessary due to a mono bug which prevents the use of the unmanaged type constraint.
	// https://github.com/mono/mono/issues/10144

	// For more information and original source, see the following answer.
	// https://stackoverflow.com/a/53969182

	[SerializableAttribute]
	public static class UnmanagedTypeExtensions
	{
		// Reflection is SLOW, so cache results
		private static Dictionary<Type, bool> m_CachedTypes = new Dictionary<Type, bool>();

		public static bool IsUnmanaged(this Type type)
		{
			bool isUnmanaged = false;

			if (type.IsPrimitive || type.IsPointer || type.IsEnum)
			{
				isUnmanaged = true;
			}
			else if (type.IsGenericType || !type.IsValueType)
			{
				isUnmanaged = false;
			}
			else
			{
				if (m_CachedTypes.TryGetValue(type, out isUnmanaged)) return isUnmanaged;

				FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				isUnmanaged = fields.All(x => x.FieldType.IsUnmanaged());

				m_CachedTypes.Add(type, isUnmanaged);
			}

			return isUnmanaged;
		}
	}
}
