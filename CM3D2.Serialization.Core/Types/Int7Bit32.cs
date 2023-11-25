using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Types
{
    /// <summary>
    /// A 7-Bit Encoded Int32
    /// </summary>
    public struct Int7Bit32 : ICM3D2Serializable, IComparable, IFormattable, IConvertible, IComparable<Int7Bit32>, IEquatable<Int7Bit32>, IComparable<int>, IEquatable<int>
    {
        [DeepSerialized]
        private int m_Value;

		public void ReadWith(ICM3D2Reader reader)
		{
            // https://github.com/mono/mono/blob/59a40d68879b69416f15a1d78198483cddda8cc0/mcs/class/referencesource/mscorlib/system/io/binaryreader.cs#L626-L644
            // Creative Commons Attribution-Share Alike 3.0 United States License

            // Read out an Int32 7 bits at a time.  The high bit
            // of the byte when on means to continue reading more bytes.
            m_Value = 0;
            int shift = 0;
            byte b;
            do
            {
                // Check for a corrupted stream.  Read a max of 5 bytes.
                // In a future version, add a DataFormatException.
                if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                    throw new FormatException("Too many bytes in what should have been a 7 bit encoded Int32.");

                // ReadByte handles end of stream cases for us.
                reader.Read(out b);
                m_Value |= (b & 0x7F) << shift;
                shift += 7;
            }
            while ((b & 0x80) != 0);
        }

        public void WriteWith(ICM3D2Writer writer)
        {
            // https://github.com/mono/mono/blob/59a40d68879b69416f15a1d78198483cddda8cc0/mcs/class/referencesource/mscorlib/system/io/binarywriter.cs#L462-L471
            // Creative Commons Attribution-Share Alike 3.0 United States License


            // Write out an int 7 bits at a time.  The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = (uint)m_Value;   // support negative numbers
            unchecked
            {
                while (v >= 0x80)
                {
                    writer.Write((byte)(v | 0x80));
                    v >>= 7;
                }
            }

            writer.Write((byte)v);
		}







		public override bool Equals(object obj)
        {
            if (obj is Int7Bit32 int7bit32)
            {
                return m_Value.Equals(int7bit32.m_Value);
            }
            else
            {
                return m_Value.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return m_Value.GetHashCode();
        }

        public int CompareTo(Int7Bit32 other)
        {
            return m_Value.CompareTo(other.m_Value);
        }

        public bool Equals(Int7Bit32 other)
        {
            return m_Value.Equals(other.m_Value);
        }

        public static bool operator >(Int7Bit32 lhs, int rhs)
        {
            return lhs.m_Value > rhs;
        }
        public static bool operator <(Int7Bit32 lhs, int rhs)
        {
            return lhs.m_Value < rhs;
        }
        public static bool operator >=(Int7Bit32 lhs, int rhs)
        {
            return lhs.m_Value >= rhs;
        }
        public static bool operator <=(Int7Bit32 lhs, int rhs)
        {
            return lhs.m_Value <= rhs;
        }
        public static bool operator ==(Int7Bit32 lhs, int rhs)
        {
            return lhs.m_Value == rhs;
        }
        public static bool operator !=(Int7Bit32 lhs, int rhs)
        {
            return lhs.m_Value != rhs;
        }
        public static bool operator >(Int7Bit32 lhs, Int7Bit32 rhs)
        {
            return lhs.m_Value > rhs.m_Value;
        }
        public static bool operator <(Int7Bit32 lhs, Int7Bit32 rhs)
        {
            return lhs.m_Value < rhs.m_Value;
        }
        public static bool operator >=(Int7Bit32 lhs, Int7Bit32 rhs)
        {
            return lhs.m_Value >= rhs.m_Value;
        }
        public static bool operator <=(Int7Bit32 lhs, Int7Bit32 rhs)
        {
            return lhs.m_Value <= rhs.m_Value;
        }
        public static bool operator ==(Int7Bit32 lhs, Int7Bit32 rhs)
        {
            return lhs.m_Value == rhs.m_Value;
        }
        public static bool operator !=(Int7Bit32 lhs, Int7Bit32 rhs)
        {
            return lhs.m_Value != rhs.m_Value;
        }

        public static explicit operator Int7Bit32(int from)
        {
            return new Int7Bit32() { m_Value = from };
        }

        public static implicit operator int(Int7Bit32 from)
        {
            return from.m_Value;
        }

        public int CompareTo(object obj)
            => m_Value.CompareTo(obj);

        public int CompareTo(int other)
            => m_Value.CompareTo(other);

        public bool Equals(int other)
            => m_Value.Equals(other);

        public TypeCode GetTypeCode()
            => m_Value.GetTypeCode();

        public string ToString(string format, IFormatProvider formatProvider)
            => m_Value.ToString(format, formatProvider);

        public string ToString(IFormatProvider provider)
            => m_Value.ToString(provider);

        bool IConvertible.ToBoolean(IFormatProvider provider)
            => ((IConvertible)m_Value).ToBoolean(provider);

        byte IConvertible.ToByte(IFormatProvider provider)
            => ((IConvertible)m_Value).ToByte(provider);

        char IConvertible.ToChar(IFormatProvider provider)
            => ((IConvertible)m_Value).ToChar(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
            => ((IConvertible)m_Value).ToDateTime(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider)
            => ((IConvertible)m_Value).ToDecimal(provider);

        double IConvertible.ToDouble(IFormatProvider provider)
            => ((IConvertible)m_Value).ToDouble(provider);

        short IConvertible.ToInt16(IFormatProvider provider)
            => ((IConvertible)m_Value).ToInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider)
            => ((IConvertible)m_Value).ToInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider)
            => ((IConvertible)m_Value).ToInt64(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider)
            => ((IConvertible)m_Value).ToSByte(provider);

        float IConvertible.ToSingle(IFormatProvider provider)
            => ((IConvertible)m_Value).ToSingle(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
            => ((IConvertible)m_Value).ToType(conversionType, provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider)
            => ((IConvertible)m_Value).ToUInt16(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider)
            => ((IConvertible)m_Value).ToUInt32(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider)
            => ((IConvertible)m_Value).ToUInt64(provider);
    }
}
