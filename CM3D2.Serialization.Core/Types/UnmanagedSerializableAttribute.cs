using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Types
{
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	public sealed class UnmanagedSerializableAttribute : Attribute
	{
	}
}
