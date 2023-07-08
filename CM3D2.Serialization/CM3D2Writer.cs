using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

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

		public void Write(string str, Encoding encoding = null)
		{
			if (encoding == null) encoding = Encoding.UTF8;
			byte[] bytes = encoding.GetBytes(str);
			Write((Int7Bit32)bytes.Length);
			m_Stream.Write(bytes, 0, bytes.Length);
		}

		public void Write<T>(T val)
			where T : unmanaged
		{
			if (val is ICM3D2Serializable serializable)
			{
				serializable.WriteWith(this);
			}

			else if (val is bool boolVal)
			{
				WriteBool(boolVal); // bools should only ever be 1 byte in length
			}

			else if (!typeof(T).IsBytesCastable())
			{
				throw new ArgumentException($"Cannot write struct {typeof(T).Name} with an invalid StructLayout");
			}
			
			else if (val is byte b)
			{
				m_Stream.WriteByte(b);
			}

			else
			{
				byte[] bytes = ToBytes(val);
				m_Stream.Write(bytes, 0, bytes.Length);
			}
		}

		public void Write<T>(T? val)
			where T : unmanaged
		{
			if (val.HasValue)
			{
				Write(val.Value);
			}
		}

			protected void WriteBool(bool val)
		{
			m_Stream.WriteByte(!val ? (byte)0 : (byte)1); // bools should only ever be 1 byte in length
		}

		protected unsafe virtual byte[] ToBytes(in object structure)
		{
			Type type = structure.GetType();
			int size = Marshal.SizeOf(type);

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
