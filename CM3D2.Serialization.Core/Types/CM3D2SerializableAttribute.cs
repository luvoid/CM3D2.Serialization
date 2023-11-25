using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization
{
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
	[Obsolete("", true)]
	public sealed class CM3D2SerializableAttribute : Attribute
	{ }
}
