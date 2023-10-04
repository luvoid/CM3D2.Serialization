using System;

namespace CM3D2.Serialization.Types
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	public sealed class DeepSerializedAttribute : Attribute
	{ }
}
