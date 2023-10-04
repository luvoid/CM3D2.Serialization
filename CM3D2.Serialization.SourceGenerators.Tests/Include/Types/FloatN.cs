using System;
using System.Runtime.InteropServices;

namespace CM3D2.Serialization.Types
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
        public float x, y, z, w;
        public Float4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public override string ToString() => $"({x}, {y}, {z}, {w})";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Float4x4
    {
        public Float4 row0, row1, row2, row3;
        public Float4x4(Float4 row0, Float4 row1, Float4 row2, Float4 row3)
        {
            this.row0 = row0;
            this.row1 = row1;
            this.row2 = row2;
            this.row3 = row3;
        }
        public override string ToString() =>
            $"({row0},\n" +
            $" {row1},\n" +
            $" {row2},\n" +
            $" {row3})";
    }
}
