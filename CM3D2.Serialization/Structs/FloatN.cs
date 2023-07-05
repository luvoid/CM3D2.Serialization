using System;
using System.Runtime.InteropServices;

namespace CM3D2.Serialization.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Float2
	{
		public float x, y;
		public Float2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
		public override string ToString() => $"({x}, {y})";
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Float3
	{
		public float x, y, z;
		public Float3(float x, float y, float z) 
		{ 
			this.x = x; 
			this.y = y;
			this.z = z;
		}
		public override string ToString() => $"({x}, {y}, {z})";
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Float4
	{
		public float w, x, y, z;
		public Float4(float w, float x, float y, float z)
		{
			this.w = w;
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public override string ToString() => $"({w}, {x}, {y}, {z})";
	}
}
