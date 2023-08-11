using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.Serialization
{
	/// <summary>
	/// Exposes the same methods as <see cref="ICM3D2Serializable"/>
	/// but requires an instance to serialize into.
	/// </summary>
	public interface ICM3D2SerializableInstance
	{
		void WriteWith(ICM3D2Writer writer);
		void ReadWith(ICM3D2Reader reader);
	}
}
