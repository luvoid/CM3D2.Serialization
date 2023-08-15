using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.Serialization
{
	internal class CM3D2Writer : ICM3D2Writer
	{
		Stream m_Stream;
		
		public CM3D2Writer(Stream stream)
		{
			m_Stream = stream;
		}

		void IDisposable.Dispose()
		{
			m_Stream = null;
		}

		public void Write(ICM3D2Serializable obj)
		{
			obj.WriteWith(this);
		}

		public void Write(ICM3D2SerializableInstance obj)
		{
			obj.WriteWith(this);
		}

		public void Write(string str, Encoding encoding = null)
		{
			str ??= "";
			if (encoding == null) encoding = Encoding.UTF8;
			byte[] bytes = encoding.GetBytes(str);
			Write((Int7Bit32)bytes.Length);
			m_Stream.Write(bytes, 0, bytes.Length);
		}

		public void Write<T>(T val)
			where T : struct
		{
			if (val is ICM3D2Serializable serializable)
			{
				serializable.WriteWith(this);
			}

			else if (val is bool boolVal)
			{
				WriteBool(boolVal); // bools should only ever be 1 byte in length
			}

			else if (val is byte b)
			{
				m_Stream.WriteByte(b);
			}

			else if (!typeof(T).IsUnmanaged())
			{
				throw new ArgumentException($"Cannot write struct {typeof(T).Name} because it is not an unmanaged type.");
			}

			else if (!typeof(T).IsBytesCastable())
			{
				throw new ArgumentException($"Cannot write struct {typeof(T).Name} with an invalid StructLayout");
			}

			else
			{
				byte[] bytes = ToBytes(val);
				m_Stream.Write(bytes, 0, bytes.Length);
			}
		}

		public void Write<T>(T? val)
			where T : struct
		{
			Write(val.Value);
		}

		protected void WriteBool(bool val)
		{
			m_Stream.WriteByte(!val ? (byte)0 : (byte)1); // bools should only ever be 1 byte in length
		}

		protected unsafe virtual byte[] ToBytes<T>(in T value)
			where T : struct
		{
			Type type = typeof(T);
			int size = sizeof(T);
			object structure = value;
			if (type.IsEnum)
			{
				Type underlyingType = Enum.GetUnderlyingType(type);
				structure = Convert.ChangeType(value, underlyingType);
			}
			
			byte[] bytes = new byte[size];
			fixed (byte* ptr = bytes)
			{
				Marshal.StructureToPtr(structure, (IntPtr)ptr, true);
			}

			return bytes;
		}

		public void DebugLog(string note)
		{
#if DEBUG
			Console.Error.WriteLine($"{note} at stream position 0x{m_Stream.Position:X8}");
#endif
		}

	}
}
