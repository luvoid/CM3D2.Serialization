using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization
{
	/// <summary>
	/// This attributes specifies that a type's implementation of <see cref="ICM3D2Serializable"/>
	/// should be automatically generated. Requires CM3D2.Serialization.SourceGenerators to be included
	/// as an analyzer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class AutoCM3D2SerializableAttribute : Attribute
	{ }
}
