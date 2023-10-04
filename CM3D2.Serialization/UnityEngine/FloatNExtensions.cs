using CM3D2.Serialization.Types;
using UnityEngine;

namespace CM3D2.Serialization.UnityEngine
{
	public static class FloatNExtensions
	{
		public static Vector2 ToVector2(this Float2 float2)
		{
			return new Vector2(float2.x, float2.y);
		}

		public static Vector3 ToVector3(this Float3 float3)
		{
			return new Vector3(float3.x, float3.y, float3.z);
		}

		public static Vector4 ToVector4(this Float4 float4)
		{
			return new Vector4(float4.x, float4.y, float4.z, float4.w);
		}

		public static Quaternion ToQuaternion(this Float4 float4)
		{
			return new Quaternion(float4.x, float4.y, float4.z, float4.w);
		}

		public static Matrix4x4 ToMatrix4x4(this Float4x4 float4x4)
		{
			return new Matrix4x4()
			{
				m00 = float4x4.row0.x, m01 = float4x4.row0.y, m02 = float4x4.row0.z, m03 = float4x4.row0.w,
				m10 = float4x4.row1.x, m11 = float4x4.row1.y, m12 = float4x4.row1.z, m13 = float4x4.row1.w,
				m20 = float4x4.row2.x, m21 = float4x4.row2.y, m22 = float4x4.row2.z, m23 = float4x4.row2.w,
				m30 = float4x4.row3.x, m31 = float4x4.row3.y, m32 = float4x4.row3.z, m33 = float4x4.row3.w,
			};
		}
	}
}
