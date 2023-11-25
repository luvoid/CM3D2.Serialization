using System;

namespace CM3D2.Serialization.Types
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class DeepSerializedAttribute : Attribute
	{ }
}
