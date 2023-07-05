using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization
{
	internal interface ICM3D2Serializable
	{
		void WriteWith(CM3D2Formatter formatter);
		void ReadWith(CM3D2Formatter formatter);
	}
}
