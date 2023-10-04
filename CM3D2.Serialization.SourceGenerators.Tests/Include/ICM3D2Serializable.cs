using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization
{
	public interface ICM3D2Serializable
	{
		void WriteWith(ICM3D2Writer writer);
		void ReadWith(ICM3D2Reader reader);
	}
}
