using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.Serialization.Types
{
	[AutoCM3D2Serializable]
	public partial class Morph : ICM3D2Serializable
	{
		public string name;
		public LengthPrefixedArray<Vertex> vertices;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Vertex
		{
			public ushort index;
			public Float3 position;
			public Float3 normal;
		}
	}
}
